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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Asyncify.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConsiderAwaitOverBlockingTaskResultCodeFixProvider)), Shared]
    public class ConsiderAwaitOverBlockingTaskResultCodeFixProvider : CodeFixProvider
    {
        private const string title = "await Task.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ConsiderAwaitOverBlockingTaskResultAnalyzer.DiagnosticId); }
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

            // Name check for Result Task property
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.ResultProperty))
            {

                // Register a code action that will invoke the fix for .Result
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                    createChangedDocument: c => FixResultProperty(root, context.Document, identifierNameSyntax, c),
                    equivalenceKey: AsyncifyResources.KeyUseAwaitResult),
                    diagnostic);
            }
            
        }
        

        private Task<Document> FixResultProperty(SyntaxNode root, Document document, IdentifierNameSyntax resultPropertySyntax, CancellationToken cancellationToken)
        {
            
            var blockingTaskExpression = resultPropertySyntax.Ancestors().OfType<MemberAccessExpressionSyntax>().First();
            
            // Grab first expression before the .Result call
            var keepTaskNode = (ExpressionSyntax) blockingTaskExpression.DescendantNodes().First();
            
            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            // Cant call .Result off of a non generic Task.
            var newRoot = blockingTaskExpression.AwaitBlockingTaskSyntax(syntaxEditor, keepTaskNode, true);

            // Replace the old node
            var newDocument = document.WithSyntaxRoot(newRoot);
            return Task.FromResult(newDocument);
        }

        

        
        
    }
}