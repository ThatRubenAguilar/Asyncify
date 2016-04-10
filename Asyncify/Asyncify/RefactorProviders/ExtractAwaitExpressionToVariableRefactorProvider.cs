using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asyncify.Extensions;
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
                awaitExpr.Ancestors().FirstOrDefault(n => n is BlockSyntax || n is ParenthesizedLambdaExpressionSyntax);

            if (containingSyntax is BlockSyntax)
            {
                // Something is using a nested await expression, offer to extract it.
                var action = CodeAction.Create(title, c => ExtractAwaitToVariableFromBlock(root, context.Document, awaitExpr, (BlockSyntax)containingSyntax, c));

                context.RegisterRefactoring(action);
            }
            else if (containingSyntax is ParenthesizedLambdaExpressionSyntax)
            {
                // Something is using a nested await expression, offer to extract it.
                var action = CodeAction.Create(title, c => ExtractAwaitToVariableFromSingleLineLambda(root, context.Document, awaitExpr, (ParenthesizedLambdaExpressionSyntax)containingSyntax, c));

                context.RegisterRefactoring(action);
            }

        }

        private async Task<Document> ExtractAwaitToVariableFromSingleLineLambda(SyntaxNode root, Document document, AwaitExpressionSyntax awaitExpr, ParenthesizedLambdaExpressionSyntax containingLambda, CancellationToken cancellationToken)
        {
            // Single Line Lambda Logic: add a block, and add semicolon and return the value we are swapping
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var lambdaInfo = semanticModel.GetSymbolInfo(containingLambda, cancellationToken);
            var lambdaSymbol = lambdaInfo.Symbol as IMethodSymbol;

            // Check lambda return type, return type is always non void due to extract only applying to await expressions that are used by something. 
            // Since it has await lambda is always async
            if (lambdaSymbol == null || !lambdaSymbol.IsAsync || lambdaSymbol.ReturnsVoid)
                return document;
            
            var lambdaBody = containingLambda.ChildNodes().OfType<ExpressionSyntax>().LastOrDefault();
            if (lambdaBody == null)
                return document;
            
            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            var lambdaReturnStatement = SyntaxFactory.ReturnStatement(lambdaBody);
            var lambdaBlockBody = SyntaxFactory.Block(new[] {lambdaReturnStatement});

            syntaxEditor.ReplaceNode(lambdaBody, lambdaBlockBody);
            
            var containingExpr = containingLambda.ChildNodes().FirstOrDefault(n => n.Contains(awaitExpr));

            // Houdini await expression somehow disappeared, but lets not crash
            if (containingExpr == null)
                return document;

            // Extract await to local variable
            // TODO: Replace in lambdaBody with identifier for extract var
            // TODO: Insert type decl in array in block
            return document;

        }

        private async Task<Document> ExtractAwaitToVariableFromBlock(SyntaxNode root, Document document, AwaitExpressionSyntax awaitExpr, BlockSyntax containingSyntax, CancellationToken cancellationToken)
        {
            // Block logic: find smallest symbol in children containing the original await and move it up with a variable addition.

            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            return await ExtractAwaitToVariable(document, syntaxEditor, awaitExpr, containingSyntax, cancellationToken);
        }

        private static async Task<Document> ExtractAwaitToVariable(Document document, SyntaxEditor syntaxEditor, AwaitExpressionSyntax awaitExpr, SyntaxNode containingSyntax, CancellationToken cancellationToken)
        {
            var containingExpr = containingSyntax.ChildNodes().FirstOrDefault(n => n.Contains(awaitExpr));

            // Houdini await expression somehow disappeared, but lets not crash
            if (containingExpr == null)
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // Get type returned by await expression
            var awaitedType = awaitExpr.GetAwaitedType(semanticModel);

            // This refactoring should only be picked up by nested calls which should only be possible on generic tasks, but avoid crashes anyway
            if (awaitedType == null)
            {
                return document;
            }

            var defaultVarName = awaitExpr.GenerateDefaultUnusedLocalVariableName("taskResult", semanticModel);

            var newRoot = awaitExpr.ExtractAwaitExpressionToVariable(syntaxEditor, semanticModel, containingExpr,
                awaitedType, defaultVarName);
            // Replace the old node
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}