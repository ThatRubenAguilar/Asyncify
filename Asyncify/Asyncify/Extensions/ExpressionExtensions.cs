using System;
using System.Diagnostics;
using System.Linq;
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
        /// <param name="semanticModel">Semantic model of the expression</param>
        /// <param name="variableType">Type of the extracted variable</param>
        /// <param name="varName">Name of the variable</param>
        /// <param name="mergeSurroundingTrivia">Merge edge trivia from parent of expression into the variable declaration</param>
        /// <returns>Local declaration statement syntax with extracted expression.</returns>
        public static LocalDeclarationStatementSyntax ExtractToLocalVariable(this ExpressionSyntax expr, SemanticModel semanticModel, ITypeSymbol variableType, string varName, bool mergeSurroundingTrivia = false)
        {
            var minimalTypeString = variableType.ToMinimalDisplayString(semanticModel,
                expr.SpanStart);

            // Create full local declaration by moving the expression
            var equalsExpression = mergeSurroundingTrivia ? expr.MergeSurroundingTrivia() : expr;
            var varTypeDeclaration = SyntaxFactory.IdentifierName(minimalTypeString);
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
        /// <param name="semanticModel">Semantic model of the await expression tree</param>
        /// <param name="awaitContainingExpr">Expression which fully contains the await expression, used to position the insert for the local variable declaration</param>
        /// <param name="awaitedType">Unwrapped type returned by the await expression</param>
        /// <param name="varName">Name of the local variable</param>
        /// <param name="mergeTriviaSurroundingAwait">Whether to merge in trivia around the await expression or not</param>
        /// <returns>New modified root after extraction</returns>
        public static SyntaxNode ExtractAwaitExpressionToVariable(this AwaitExpressionSyntax awaitExpr, SyntaxEditor syntaxEditor, SemanticModel semanticModel, SyntaxNode awaitContainingExpr, ITypeSymbol awaitedType, string varName, bool mergeTriviaSurroundingAwait = false)
        {

            LocalDeclarationStatementSyntax extractedAwaitDeclaration = awaitExpr.ExtractToLocalVariable(semanticModel, awaitedType, varName, mergeTriviaSurroundingAwait);

            syntaxEditor.InsertBefore(awaitContainingExpr, extractedAwaitDeclaration);

            // Replace the old await expression with the new local variable
            var awaitVarIdentifierName = SyntaxFactory.IdentifierName(varName);

            if (mergeTriviaSurroundingAwait)
            {
                // TODO: Need to remove paren tokens' trivia and do the same in single line lambda
                // write the merging logic as a syntax rewriter
            }
            else
            {
                syntaxEditor.ReplaceNode(awaitExpr, awaitVarIdentifierName);
            }

            var newRoot = syntaxEditor.GetChangedRoot();
            return newRoot;
        }

        /// <summary>
        /// Merges trivia surrounding the expression into the expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static ExpressionSyntax MergeSurroundingTrivia(this ExpressionSyntax expr)
        {
            var firstInsideToken = expr.DescendantTokens().FirstOrDefault();
            var lastInsideToken = expr.DescendantTokens().LastOrDefault();
            if (firstInsideToken.IsDefault() || lastInsideToken.IsDefault())
                throw new ArgumentException($"Expression has no tokens to merge trivia into.");

            var parentNode = expr.Parent;
            var beforeFirstInsideToken = parentNode.GetTokenBeforeOrDefault(firstInsideToken);
            var afterLastInsideToken = parentNode.GetTokenAfterOrDefault(lastInsideToken);

            ExpressionSyntax returnExpression = expr;
            if (!beforeFirstInsideToken.IsDefault())
            {
                var mergedLeadingTrivia = beforeFirstInsideToken.GetMergedTrailingTrivia(firstInsideToken);
                returnExpression = returnExpression.WithLeadingTrivia(mergedLeadingTrivia);
            }
            if (!afterLastInsideToken.IsDefault())
            {
                var mergedTrailingTrivia = afterLastInsideToken.GetMergedLeadingTrivia(lastInsideToken);
                returnExpression = returnExpression.WithTrailingTrivia(mergedTrailingTrivia);
            }
            return returnExpression;
        }
    }
}