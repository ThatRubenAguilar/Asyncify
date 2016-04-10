using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asyncify.Analyzers;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Asyncify.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConsiderAwaitOverBlockingTaskGetResultCodeFixProvider)), Shared]
    public class ConsiderAwaitOverBlockingTaskGetResultCodeFixProvider : CodeFixProvider
    {
        private const string title = "Await Task.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncifyDiagnosticIds.AwaitTaskGetResultDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the identifier name identified by the diagnostic.
            var identifierNameSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IdentifierNameSyntax>().First();

            // Name check for GetResult() Task method
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.GetResultMethod))
            {
                var fullParentExpressionDescendantIdentifiers = identifierNameSyntax.Parent.DescendantNodes().OfType<IdentifierNameSyntax>().ToArray();

                // 2nd to last identifier should be GetAwaiter() walking in prefix tree order
                var getResultInvoker = fullParentExpressionDescendantIdentifiers.Length < 2
                    ? null
                    : fullParentExpressionDescendantIdentifiers[fullParentExpressionDescendantIdentifiers.Length - 2];
                // Ensure .GetAwaiter() is above the .GetResult()
                if (getResultInvoker != null && getResultInvoker.Identifier.Text.Equals(AsyncifyResources.GetAwaiterMethod))
                {
                    // Register a code action that will invoke the fix for .GetAwaiter().GetResult()
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedDocument:
                                c => FixGetResultMethod(root, context.Document,  identifierNameSyntax, getResultInvoker, c),
                            equivalenceKey: AsyncifyResources.KeyUseAwaitGetResult),
                        diagnostic);
                }
            }
            // IDEA: More complex fixes on .GetAwaiter().GetResult() should be checked and added.
        }
        
        


        private async Task<Document> FixGetResultMethod(SyntaxNode root, Document document, IdentifierNameSyntax getResultSyntax, IdentifierNameSyntax getAwaiterSyntax, CancellationToken cancellationToken)
        {
            
            // SimpleMemberAccess -> Invocation
            var blockingTaskExpression = getResultSyntax.Ancestors().OfType<InvocationExpressionSyntax>().First();
            
            // SimpleMemberAccess (GetResult) -> Invocation (GetAwaiter) -> SimpleMemberAccess (GetAwaiter) -> Invocation (Callee)
            var keepTaskNode = (ExpressionSyntax)blockingTaskExpression.DescendantNodes().Take(4).Last();


            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbolInfo = semanticModel.GetSymbolInfo(getAwaiterSyntax, cancellationToken);
            var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
            var callerIsGenericTask = AsyncifyResources.TaskGenericRegex.IsMatch(methodSymbol.ContainingType.ToString());
            
            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            var newRoot = blockingTaskExpression.AwaitBlockingTaskSyntax(syntaxEditor, keepTaskNode, callerIsGenericTask);

            // Replace the old node
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        
        
    }
}