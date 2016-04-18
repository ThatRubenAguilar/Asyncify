using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Asyncify.Contexts;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

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
                var action = CodeAction.Create(title, c =>
                {
                    var refactorContext = new ExtractAwaitContext(new DocumentContext(root, context.Document, c))
                    {
                        OriginalContainingBodySyntax = containingSyntax,
                        TargetAwaitExpression = awaitExpr
                    };
                    return ExtractSemanticInfoForAwaitToVariable(refactorContext);
                });

                context.RegisterRefactoring(action);
            }

        }
        /// <summary>
        /// Extracts semantic information from original tree to avoid reparsing.
        /// </summary>
        /// <returns></returns>
        private async Task<Document> ExtractSemanticInfoForAwaitToVariable(ExtractAwaitContext context)
        {
            var semanticModel = await context.DocumentContext.GetSemanticModel().ConfigureAwait(false);
            
            // Get type returned by await expression
            var awaitedType = context.TargetAwaitExpression.GetAwaitedType(semanticModel);

            // This refactoring should only be picked up by nested calls which should only be possible on generic tasks, but avoid crashes anyway
            if (awaitedType == null)
                return context.DocumentContext.Document;
            // Semantic evaluations come first as doing them off the modified tree requires a recompile of the document.
            context.SemanticContext = VariableSemanticContext.CreateForLocalVariable(context.TargetAwaitExpression, awaitedType, "taskResult", semanticModel);
             
            var containingLambda = context.OriginalContainingBodySyntax.FindContainingLambda();
            // is single line lambda
            if (containingLambda != null && containingLambda.Equals(context.OriginalContainingBodySyntax))
            {
                context.ContainingBodySyntax = containingLambda;
                return await TransformExpressionLambdaForAwaitToVariable(context);
            }

            context.ContainingBodySyntax = context.OriginalContainingBodySyntax;
            return await ExtractAwaitToVariable(context);
        }


        /// <summary>
        /// Transforms an expression lambda into a block lambda.
        /// </summary>
        /// <returns></returns>
        private static Task<Document> TransformExpressionLambdaForAwaitToVariable(ExtractAwaitContext context)
        {
            var containingLambda = context.ContainingBodySyntax as LambdaExpressionSyntax;
            if (containingLambda == null)
                return context.DocumentContext.Document.AsTask();
            // Single Line Lambda Logic: transform to block lambda then process.
            var originalLambdaBody = containingLambda.GetLambdaBody();
            if (originalLambdaBody == null)
                return context.DocumentContext.Document.AsTask();

            var blockLambda = containingLambda.ToBlockLambda(context.SemanticContext.Model,
                context.DocumentContext.Workspace,
                context.DocumentContext.Token);

            if (blockLambda == null)
                return context.DocumentContext.Document.AsTask();

            var blockSyntax = blockLambda.ChildNodes().OfType<BlockSyntax>().LastOrDefault();
            if (blockSyntax == null)
                return context.DocumentContext.Document.AsTask();

            var blockAwaitExpr =
                blockSyntax.DescendantNodes().OfType<AwaitExpressionSyntax>()
                    .FirstOrDefault(
                        n => n.ToStringWithoutTrivia().Equals(context.TargetAwaitExpression.ToStringWithoutTrivia()));

            if (blockAwaitExpr == null)
                return context.DocumentContext.Document.AsTask();

            context.OriginalContainingBodySyntax = originalLambdaBody;
            context.ContainingBodySyntax = blockSyntax;
            context.TargetAwaitExpression = blockAwaitExpr;

            return ExtractAwaitToVariable(context);
        }

        /// <summary>
        /// Merges trivia and extracts the await expression to own variable within a block body.
        /// </summary>
        /// <returns></returns>
        private static Task<Document> ExtractAwaitToVariable(ExtractAwaitContext context)
        {
            // Merge trivia into expression to be extracted.
            var awaitIndex = context.ContainingBodySyntax.GetDescendantIndex(context.TargetAwaitExpression);

            var triviaMergedContainingSyntax = context.TargetAwaitExpression.MergeEdgeTriviaIn(context.ContainingBodySyntax);

            var triviaMergedAwaitExpr =
                triviaMergedContainingSyntax.GetDescendantNodeAtIndex<AwaitExpressionSyntax>(awaitIndex);

            if (triviaMergedAwaitExpr == null)
                return context.DocumentContext.Document.AsTask();


            // Block logic: find smallest symbol in children containing the original await and move it up with a variable addition.

            var containingExpr = triviaMergedContainingSyntax.ChildNodes()
                .FirstOrDefault(n => n.Contains(triviaMergedAwaitExpr));

            // Houdini await expression somehow disappeared, but lets not crash
            if (containingExpr == null)
                return context.DocumentContext.Document.AsTask();
            
            var blockSyntaxEditor = new SyntaxEditor(triviaMergedContainingSyntax, context.DocumentContext.Workspace);
            var newBlock = triviaMergedAwaitExpr.ExtractAwaitExpressionToVariable(blockSyntaxEditor,
                containingExpr, context.SemanticContext);

            var syntaxEditor = context.DocumentContext.CreateSyntaxEditor();
            syntaxEditor.ReplaceNode(context.OriginalContainingBodySyntax, newBlock);
            var newRoot = syntaxEditor.GetChangedRoot();
            // Replace the old node
            var newDocument = context.DocumentContext.Document.WithSyntaxRoot(newRoot);
            return Formatter.FormatAsync(newDocument, cancellationToken:context.DocumentContext.Token);
        }

        class ExtractAwaitContext : RefactoringContext<DocumentContext, VariableSemanticContext>
        {
            public ExtractAwaitContext(DocumentContext documentContext) : base(documentContext, null)
            {
            }

            /// <summary>
            /// Original body node which is to be replaced by the refactored node.
            /// </summary>
            public SyntaxNode OriginalContainingBodySyntax { get; set; }
            /// <summary>
            /// Block body which contains the TargetAwaitExpression
            /// </summary>
            public SyntaxNode ContainingBodySyntax { get; set; }
            /// <summary>
            /// Await expression targetted for refactoring.
            /// </summary>
            public AwaitExpressionSyntax TargetAwaitExpression { get; set; }
        }


    }
}