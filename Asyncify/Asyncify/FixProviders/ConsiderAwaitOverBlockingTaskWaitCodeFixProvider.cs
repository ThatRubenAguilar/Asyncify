using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Asyncify.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConsiderAwaitOverBlockingTaskWaitCodeFixProvider)), Shared]
    public class ConsiderAwaitOverBlockingTaskWaitCodeFixProvider : CodeFixProvider
    {
        private const string title = "Await Task.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncifyDiagnosticIds.AwaitTaskWaitDiagnosticId); }
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

            // Name check for Wait() Task method
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.WaitMethod))
            {
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                var symbolInfo = semanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);
                var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;

                // Only want .Wait() or .Wait(CancellationToken)
                if (methodSymbol.Parameters.Length == 0 || methodSymbol.DoParametersMatch(new []{ typeof(CancellationToken) }))
                {
                    // Register a code action that will invoke the fix for .Wait()
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedDocument:
                                c => FixWaitMethod(root, context.Document, identifierNameSyntax, c),
                            equivalenceKey: AsyncifyResources.KeyUseAwaitGetResult),
                        diagnostic);
                }
                // IDEA: More fixes on different overloads of .Wait()
            }
        }
        
        


        private Task<Document> FixWaitMethod(SyntaxNode root, Document document, IdentifierNameSyntax waitSyntax, CancellationToken cancellationToken)
        {
            
            // SimpleMemberAccess -> Invocation
            var blockingTaskExpression = waitSyntax.Ancestors().OfType<InvocationExpressionSyntax>().First();
            
            // SimpleMemberAccess (Wait) -> Invocation (Callee)
            var keepTaskNode = (ExpressionSyntax)blockingTaskExpression.DescendantNodes().Take(2).Last();
            
            
            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            // This fix provider is invoked only on nongeneric tasks
            var newRoot = blockingTaskExpression.AwaitBlockingTaskSyntax(syntaxEditor, keepTaskNode, false);

            // Replace the old node
            var newDocument = document.WithSyntaxRoot(newRoot);
            return Formatter.FormatAsync(newDocument, cancellationToken: cancellationToken);
        }

        
        
    }
}