using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asyncify.Extensions;
using Asyncify.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;

namespace Asyncify.RefactorProviders
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ExtractAwaitExpressionToVariableRefactorProvider)), Shared]
    public class ExtractAwaitExpressionToVariableRefactorProvider : CodeRefactoringProvider
    {
        const string title = "Extract await to variable.";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a await expression.
            var awaitExpr = node as AwaitExpressionSyntax;
            if (awaitExpr == null)
            {
                return;
            }
            
            AnalyzeAwaitExtract(root, context, awaitExpr);
        }

        private void AnalyzeAwaitExtract(SyntaxNode root, CodeRefactoringContext context, AwaitExpressionSyntax awaitExpr)
        {
            var awaitExprAccessSyntax = awaitExpr.Ancestors().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();

            if (awaitExprAccessSyntax == null)
                return;


            // Find parent block or parenthesized lambda
            var containingSyntax =
                awaitExpr.Ancestors().FirstOrDefault(n => n is BlockSyntax || n is LambdaExpressionSyntax);

            if (containingSyntax is BlockSyntax || containingSyntax is LambdaExpressionSyntax)
            {
                // Something is using a nested await expression, offer to extract it.
                var action = CodeAction.Create(title, c => ExtractSemanticInfoForAwaitToVariable(root, context.Document, awaitExpr, containingSyntax, c));

                context.RegisterRefactoring(action);
            }

        }
        /// <summary>
        /// Extracts semantic information from original tree to avoid reparsing.
        /// </summary>
        /// <param name="awaitExpr">await expression targetted by refactoring for extract</param>
        /// <param name="originalContainingSyntax">syntax which contains the await expression</param>
        /// <returns></returns>
        private async Task<Document> ExtractSemanticInfoForAwaitToVariable(SyntaxNode root, Document document, AwaitExpressionSyntax awaitExpr, SyntaxNode originalContainingSyntax, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            
            // Get type returned by await expression
            var awaitedType = awaitExpr.GetAwaitedType(semanticModel);

            // This refactoring should only be picked up by nested calls which should only be possible on generic tasks, but avoid crashes anyway
            if (awaitedType == null)
                return document;
            // Semantic evaluations come first as doing them off the modified tree requires a recompile of the document.
            var defaultVarName = awaitExpr.GenerateDefaultUnusedLocalVariableName("taskResult", semanticModel);

            var minimalTypeString = awaitedType.ToMinimalDisplayString(semanticModel,
                awaitExpr.SpanStart);
            var varTypeDeclaration = SyntaxFactory.IdentifierName(minimalTypeString);


            var containingLambda = originalContainingSyntax.FindContainingLambda();
            // is single line lambda
            if (containingLambda != null && containingLambda.Equals(originalContainingSyntax))
            {
                return TransformExpressionLambdaForAwaitToVariable(root, document, awaitExpr, containingLambda, semanticModel, varTypeDeclaration, defaultVarName, cancellationToken);
            }

            return ExtractAwaitToVariable(root, document, originalContainingSyntax, originalContainingSyntax, awaitExpr, varTypeDeclaration, defaultVarName);
        }

        /// <summary>
        /// Transforms an expression lambda into a block lambda.
        /// </summary>
        /// <param name="awaitExpr">await expression targetted by refactoring for extract</param>
        /// <param name="containingLambda">lambda expression node that contains the await expression</param>
        /// <param name="semanticModel">semantic model of the original syntax tree</param>
        /// <param name="varTypeDeclaration">variable type node that is the return type of the awaited expression</param>
        /// <param name="defaultVarName">variable name for the extracted await expression</param>
        /// <returns></returns>
        private static Document TransformExpressionLambdaForAwaitToVariable(SyntaxNode root, Document document,
            AwaitExpressionSyntax awaitExpr, LambdaExpressionSyntax containingLambda, SemanticModel semanticModel,
            IdentifierNameSyntax varTypeDeclaration, string defaultVarName, CancellationToken cancellationToken)
        {
            // Single Line Lambda Logic: transform to block lambda then process.
            var originalLambdaBody = containingLambda.GetLambdaBody();
            if (originalLambdaBody == null)
                return document;

            var blockLambda = containingLambda.ToBlockLambda(semanticModel,
                document.Project.Solution.Workspace,
                cancellationToken);

            if (blockLambda == null)
                return document;

            var blockSyntax = blockLambda.ChildNodes().OfType<BlockSyntax>().LastOrDefault();
            if (blockSyntax == null)
                return document;

            var blockAwaitExpr =
                blockSyntax.DescendantNodes().OfType<AwaitExpressionSyntax>()
                    .FirstOrDefault(
                        n => n.ToStringWithoutTrivia().Equals(awaitExpr.ToStringWithoutTrivia()));

            if (blockAwaitExpr == null)
                return document;

            return ExtractAwaitToVariable(root, document, originalLambdaBody, blockSyntax, blockAwaitExpr, varTypeDeclaration,
                defaultVarName);
        }

        /// <summary>
        /// Merges trivia and extracts the await expression to own variable within a block body.
        /// </summary>
        /// <param name="originalContainingBodySyntax">original block body to replace</param>
        /// <param name="containingSyntax">block syntax which contains the await expression</param>
        /// <param name="awaitExpr">await expression targetted by refactoring for extract</param>
        /// <param name="varTypeDeclaration">variable type node that is the return type of the awaited expression</param>
        /// <param name="extractVarName">variable name for the extracted await expression</param>
        /// <returns></returns>
        private static Document ExtractAwaitToVariable(SyntaxNode root, Document document, 
            SyntaxNode originalContainingBodySyntax, SyntaxNode containingSyntax, AwaitExpressionSyntax awaitExpr,
            IdentifierNameSyntax varTypeDeclaration, string extractVarName)
        {
            // Merge trivia into expression to be extracted.
            var awaitIndex = containingSyntax.GetDescendantIndex(awaitExpr);

            var triviaMergedContainingSyntax = awaitExpr.MergeEdgeTriviaIn(containingSyntax);

            var triviaMergedAwaitExpr =
                triviaMergedContainingSyntax.GetDescendantNodeAtIndex<AwaitExpressionSyntax>(awaitIndex);

            if (triviaMergedAwaitExpr == null)
                return document;


            // Block logic: find smallest symbol in children containing the original await and move it up with a variable addition.

            var containingExpr = triviaMergedContainingSyntax.ChildNodes()
                .FirstOrDefault(n => n.Contains(triviaMergedAwaitExpr));

            // Houdini await expression somehow disappeared, but lets not crash
            if (containingExpr == null)
                return document;
            
            var blockSyntaxEditor = new SyntaxEditor(triviaMergedContainingSyntax, document.Project.Solution.Workspace);
            var newBlock = triviaMergedAwaitExpr.ExtractAwaitExpressionToVariable(blockSyntaxEditor,
                containingExpr, varTypeDeclaration, extractVarName);

            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);
            syntaxEditor.ReplaceNode(originalContainingBodySyntax, newBlock);
            var newRoot = syntaxEditor.GetChangedRoot();
            // Replace the old node
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}