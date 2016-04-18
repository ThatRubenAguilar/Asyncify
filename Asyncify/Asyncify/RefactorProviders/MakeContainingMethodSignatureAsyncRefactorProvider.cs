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
            
            AnalyzeMakeMethodAsync(root, context, methodSyntax);
        }

        private void AnalyzeMakeMethodAsync(SyntaxNode root, CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax)
        {

            // TODO: Implement 
            /*
                Open ?: Do we want to split Task only/async included refactorings?

                Logic: check if method is already async and avoid
                find return value of method
                look through method for awaited expressions in the method flow
                 wrap return value in Task 
                if an await exists, put async
                if no await modify non task return statements to be wrapped with Task.FromResult
                import using Task if not existing.
            */
            

            
            if (false)
            {
                var action = CodeAction.Create(title, c =>
                {
                    var refactorContext = new MakeMethodAsyncContext(new DocumentContext(root, context.Document, c))
                    {
                        OriginalMethodDeclaration = null
                    };
                    return context.Document.AsTask();
                });

                context.RegisterRefactoring(action);
            }

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