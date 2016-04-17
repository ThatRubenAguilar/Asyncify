using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class ExpressionExtensions
    {

        /// <summary>
        /// Converts a blocking task such as Task.Result/GetAwaiter().GetResult() into an awaited statement.
        /// The awaited statement is parenthesized when caller task is generic, the result of the task is used, and the task expression isnt already parenthesized.
        /// </summary>
        /// <param name="blockingTaskExpression">Origin expression that contains the task node and blocking task call as children</param>
        /// <param name="syntaxEditor">Editor to change the document</param>
        /// <param name="keepTaskNode">Calling task node that will not be removed from the blockingTaskExpression</param>
        /// <param name="callerIsGenericTask">Whether the task in keepTaskNode is a generic task type or not</param>
        /// <returns></returns>
        public static SyntaxNode AwaitBlockingTaskSyntax(this ExpressionSyntax blockingTaskExpression, SyntaxEditor syntaxEditor, ExpressionSyntax keepTaskNode, bool callerIsGenericTask)
        {
            var excludeKeepNodeTokens = keepTaskNode.DescendantTokens().ToArray();

            var triviaList = blockingTaskExpression.ExtractTrivia(excludeKeepNodeTokens);

            if (keepTaskNode.HasTrailingTrivia)
                triviaList.InsertRange(0, keepTaskNode.GetTrailingTrivia());

            var alreadyParenthesized = blockingTaskExpression.Parent as ParenthesizedExpressionSyntax;
            // last node should be relative parent unless something comes after
            var resultUsed = !blockingTaskExpression.Parent.ChildNodes().Last().Equals(blockingTaskExpression);

            // Surround with parenthesis only when the result is used and it is a generic task and it isnt already parenthesized
            if (resultUsed && alreadyParenthesized == null && callerIsGenericTask)
            {
                // Move trivia from keep node tokens to outside parens as it more closely resembles what would be expected
                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepTaskNode.WithoutTrivia());
                var newParenExpr =
                    SyntaxFactory.ParenthesizedExpression(newAwaitExpr)
                        .WithLeadingTrivia(keepTaskNode.GetLeadingTrivia())
                        .WithTrailingTrivia(triviaList);

                syntaxEditor.ReplaceNode(blockingTaskExpression, newParenExpr);
            }
            else
            {
                // Move trivia up with the removal of .GetAwaiter().GetResult()/.Result, we are already in parens or dont need parens so dont mess with it.
                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepTaskNode.WithoutTrivia())
                    .WithLeadingTrivia(keepTaskNode.GetLeadingTrivia())
                    .WithTrailingTrivia(triviaList);

                syntaxEditor.ReplaceNode(blockingTaskExpression, newAwaitExpr);
            }

            return syntaxEditor.GetChangedRoot();
        }

        /// <summary>
        /// Generates an unused local variable name
        /// </summary>
        /// <param name="expr">Expression whose location should be searched for local variable visibility</param>
        /// <param name="defaultName">Name used as prefix if it is used already</param>
        /// <param name="semanticModel">Semantic model of the document</param>
        /// <returns>A variable name unused locally</returns>
        public static string GenerateDefaultUnusedLocalVariableName(this ExpressionSyntax expr,
            string defaultName, SemanticModel semanticModel)
        {
            var defaultVarName = defaultName;

            var visibleVars = semanticModel.LookupSymbols(expr.SpanStart, name: defaultVarName);
            int counter = 1;
            while (!visibleVars.IsEmpty)
            {
                defaultVarName = $"{defaultName}{counter}";
                counter++;
                visibleVars = semanticModel.LookupSymbols(expr.SpanStart, name: defaultVarName);
            }
            return defaultVarName;
        }

        /// <summary>
        /// Returns the unwrapped type that an awaited expression returns.
        /// </summary>
        /// <param name="awaitExpr">await expression to extract type from</param>
        /// <param name="model">semantic model</param>
        /// <returns>unwrapped type that expr returns or null if it returns void</returns>
        public static ITypeSymbol GetAwaitedType(this AwaitExpressionSyntax awaitExpr, SemanticModel model)
        {
            var callerSymbol = model.GetAwaitExpressionInfo(awaitExpr);

            if (callerSymbol.GetResultMethod.ReturnsVoid)
                return null;
            
            return callerSymbol.GetResultMethod.ReturnType;
        }

        /// <summary>
        /// Extracts out an expression to a local variable declaration
        /// </summary>
        /// <param name="expr">Expression to extract</param>
        /// <param name="varTypeDeclaration">Name of the type of variable to extract to</param>
        /// <param name="varName">Name of the variable</param>
        /// <returns>Local declaration statement syntax with extracted expression.</returns>
        public static LocalDeclarationStatementSyntax ExtractToLocalVariable(this ExpressionSyntax expr, IdentifierNameSyntax varTypeDeclaration, string varName)
        {

            // Create full local declaration by moving the expression
            var equalsExpression = expr;
            
            var varEqualsClause = SyntaxFactory.EqualsValueClause(equalsExpression);
            var varIdentifier = SyntaxFactory.Identifier(varName);
            var varDeclarator = SyntaxFactory.VariableDeclarator(varIdentifier, null, varEqualsClause);

            var varDeclaratorList = SyntaxFactory.SeparatedList(new[]
            {
                varDeclarator
            });

            var extractedDeclaration =
                SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(varTypeDeclaration,
                    varDeclaratorList));
            return extractedDeclaration;
        }

        /// <summary>
        /// Extract an await expression to a local variable
        /// </summary>
        /// <param name="awaitExpr">Await expression to extract</param>
        /// <param name="syntaxEditor">Editor to use</param>
        /// <param name="awaitContainingExpr">Expression which fully contains the await expression, used to position the insert for the local variable declaration</param>
        /// <param name="varTypeDeclaration">Name of the type of variable to extract to</param>
        /// <param name="varName">Name of the local variable</param>
        /// <returns>New modified root after extraction</returns>
        public static SyntaxNode ExtractAwaitExpressionToVariable(this AwaitExpressionSyntax awaitExpr, SyntaxEditor syntaxEditor, SyntaxNode awaitContainingExpr, IdentifierNameSyntax varTypeDeclaration, string varName)
        {

            LocalDeclarationStatementSyntax extractedAwaitDeclaration = awaitExpr.ExtractToLocalVariable(varTypeDeclaration, varName);

            syntaxEditor.InsertBefore(awaitContainingExpr, extractedAwaitDeclaration);

            // Replace the old await expression with the new local variable
            var awaitVarIdentifierName = SyntaxFactory.IdentifierName(varName);
            
            syntaxEditor.ReplaceNode(awaitExpr, awaitVarIdentifierName);
            
            var newRoot = syntaxEditor.GetChangedRoot();
            return newRoot;
        }

        /// <summary>
        /// Converts a single line lambda expression to block syntax. Is a no op if it is already block lambda.
        /// </summary>
        /// <param name="lambda">Single line lambda to transform.</param>
        /// <param name="semanticModel">semantic model to inspect the lambda method with</param>
        /// <param name="workspace">workspace of the document lambda is in</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Block lambda expression</returns>
        public static LambdaExpressionSyntax ToBlockLambda(this LambdaExpressionSyntax lambda, SemanticModel semanticModel, Workspace workspace, CancellationToken cancellationToken)
        {
            var lambdaBody = lambda.ChildNodes().OfType<ExpressionSyntax>().LastOrDefault();
            // not a single line lambda
            if (lambdaBody == null) 
                return lambda;

            var lambdaInfo = semanticModel.GetSymbolInfo(lambda, cancellationToken);
            var lambdaSymbol = lambdaInfo.Symbol as IMethodSymbol;

            if (lambdaSymbol == null)
                return null;

            var lambdaEditor = new SyntaxEditor(lambda, workspace);
            StatementSyntax transformedExpression;
            if (lambdaSymbol.ReturnsVoid)
            {
                transformedExpression = SyntaxFactory.ExpressionStatement(lambdaBody);
            }
            else
            {
                transformedExpression = SyntaxFactory.ReturnStatement(lambdaBody);
            }
            var blockBody = SyntaxFactory.Block(transformedExpression);
            lambdaEditor.ReplaceNode(lambdaBody, blockBody);
            return lambdaEditor.GetChangedRoot() as LambdaExpressionSyntax;
        }

        public static bool IsBlockLambda(this LambdaExpressionSyntax lambda)
        {
            var lambdaBody = lambda.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
            return lambdaBody != null;
        }

        public static SyntaxNode GetLambdaBody(this LambdaExpressionSyntax lambda)
        {
            return lambda.ChildNodes().FirstOrDefault(n => n is BlockSyntax || n is ExpressionSyntax);
        }

        public static LambdaBodyInfo GetLambdaBodyInfo(this LambdaExpressionSyntax lambda)
        {
            var lambdaBody = lambda.GetLambdaBody();
            if (lambdaBody == null)
                return null;

            var blockBody = lambdaBody as BlockSyntax;
            if (blockBody != null)
                return new LambdaBodyInfo(blockBody, lambda);

            var expressionBody = lambdaBody as ExpressionSyntax;
            if (expressionBody != null)
                return new LambdaBodyInfo(expressionBody, lambda);

            return null;
        }
    }
}