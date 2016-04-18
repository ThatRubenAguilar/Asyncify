using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Asyncify.Contexts;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Asyncify.RefactorProviders
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MakeContainingMethodSignatureAsyncRefactorProvider)), Shared]
    public class MakeContainingMethodSignatureAsyncRefactorProvider : CodeRefactoringProvider
    {
        const string title = "Make containing method signature async.";


        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a await expression.
            var methodSyntax = node as MethodDeclarationSyntax;
            if (methodSyntax == null)
            {
                return;
            }
            
            await AnalyzeMakeMethodAsync(root, context, methodSyntax);
        }

        private async Task AnalyzeMakeMethodAsync(SyntaxNode root, CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax)
        {

            /*
                Open ?: Do we want to split Task only/async included refactorings?
                Do we want to combine lambda logic with method declaration logic?

                Logic: check if method is already async and avoid
                find return value of method
                look through method for awaited expressions in the method flow
                 wrap return value in Task 
                if an await exists, put async
                if no await modify non task return statements to be wrapped with Task.FromResult
                import using Task if not existing.
            */

            // Check if await expression exists already
            var awaitExprList = methodSyntax.DescendantNodes().OfType<AwaitExpressionSyntax>().ToList();
            if (awaitExprList.Count == 0)
                return;

            // Ensure all await expressions are not within a lambda
            awaitExprList.RemoveAll(n => n.ContainedWithin<LambdaExpressionSyntax>(methodSyntax));
            if (awaitExprList.Count == 0)
                return;

            // Check if method async, dont touch already async code
            var semanticModel =
                await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            
            var methodSymbol = semanticModel.GetSymbol<IMethodSymbol>(methodSyntax, context.CancellationToken);
            if (methodSymbol == null)
                return;

            if (methodSymbol.IsAsync)
                return;

            // Check if return value is task, means we already have working non async task function
            if (!methodSymbol.ReturnsVoid && AsyncifyResources.TaskRegex.IsMatch(methodSymbol.ReturnType.ToString()))
                return;
            
            var action = CodeAction.Create(title, c =>
            {
                var refactorContext = new MakeMethodAsyncContext(new DocumentContext(root, context.Document, c))
                {
                    OriginalMethodDeclaration = null
                };
                return context.Document.AsTask();
            });

            // TODO: Implement 
            context.RegisterRefactoring(action);
            

        }
        class MakeMethodAsyncContext : RefactoringContext<DocumentContext, SemanticContext>
        {
            public MakeMethodAsyncContext(DocumentContext documentContext) : base(documentContext, null)
            {
            }

            /// <summary>
            /// Method declaration node to be replaced.
            /// </summary>
            public MethodDeclarationSyntax OriginalMethodDeclaration { get; set; }
        }


    }
}