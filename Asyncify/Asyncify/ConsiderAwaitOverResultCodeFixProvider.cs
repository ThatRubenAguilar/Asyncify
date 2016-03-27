using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;

namespace Asyncify
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConsiderAwaitOverResultCodeFixProvider)), Shared]
    public class ConsiderAwaitOverResultCodeFixProvider : CodeFixProvider
    {
        private const string title = "await Task.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ConsiderAwaitOverResultAnalyzer.DiagnosticId); }
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
            else if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.GetResultMethod))
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
        

        private Task<Document> FixResultProperty(SyntaxNode root, Document document, IdentifierNameSyntax resultPropertySyntax, CancellationToken cancellationToken)
        {
            
            var relativeParent = resultPropertySyntax.Ancestors().OfType<MemberAccessExpressionSyntax>().First();
            
            // Grab first expression before the .Result call
            var keepNode = (ExpressionSyntax) relativeParent.DescendantNodes().First();

            return Task.FromResult(AwaitTaskSyntax(root, document, keepNode, relativeParent, true));
        }



        private async Task<Document> FixGetResultMethod(SyntaxNode root, Document document, IdentifierNameSyntax getResultSyntax, IdentifierNameSyntax getAwaiterSyntax, CancellationToken cancellationToken)
        {
            
            // SimpleMemberAccess -> Invocation
            var relativeParent = getResultSyntax.Ancestors().OfType<InvocationExpressionSyntax>().First();
            
            // SimpleMemberAccess (GetResult) -> Invocation (GetAwaiter) -> SimpleMemberAccess (GetAwaiter) -> Invocation (Callee)
            var keepNode = (ExpressionSyntax)relativeParent.DescendantNodes().Take(4).Last();


            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbolInfo = semanticModel.GetSymbolInfo(getAwaiterSyntax, cancellationToken);
            var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
            var callerIsGenericTask = AsyncifyResources.TaskGenericRegex.IsMatch(methodSymbol.ContainingType.ToString());
            
            return AwaitTaskSyntax(root, document, keepNode, relativeParent, callerIsGenericTask);
        }

        

        static Document AwaitTaskSyntax(SyntaxNode root, Document document, ExpressionSyntax keepNode,
            ExpressionSyntax relativeParent, bool callerIsGenericTask)
        {
            var syntaxEditor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            var excludeKeepNodeTokens = keepNode.DescendantTokens().ToArray();

            var triviaList = relativeParent.ExtractTrivia(excludeKeepNodeTokens);


            var alreadyParenthesized = relativeParent.Parent as ParenthesizedExpressionSyntax;
            // last node should be relative parent unless something comes after
            var resultUsed = !relativeParent.Parent.ChildNodes().Last().Equals(relativeParent);

            // Surround with parenthesis only when the result is used and it is a generic task and it isnt already parenthesized
            if (resultUsed && alreadyParenthesized == null && callerIsGenericTask)
            {
                // Move trivia from keep node tokens to outside parens as it more closely resembles what would be expected
                var moveTrailingTrivia = excludeKeepNodeTokens.Last();
                if (moveTrailingTrivia.HasLeadingTrivia || moveTrailingTrivia.HasTrailingTrivia)
                    triviaList.InsertRange(0, moveTrailingTrivia.GetAllTrivia());

                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepNode.WithoutTrivia());
                var newParenExpr =
                    SyntaxFactory.ParenthesizedExpression(newAwaitExpr)
                        .WithLeadingTrivia(relativeParent.GetLeadingTrivia())
                        .WithTrailingTrivia(triviaList);

                syntaxEditor.ReplaceNode(relativeParent, (node, gen) => newParenExpr);
            }
            else
            {
                // Move trivia up with the removal of .GetAwaiter().GetResult()/.Result, we are already in parens or dont need parens so dont mess with it.
                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepNode.WithTrailingTrivia(triviaList));

                syntaxEditor.ReplaceNode(relativeParent, (node, gen) => newAwaitExpr);
            }

            // Replace the old node
            var newDocument = document.WithSyntaxRoot(syntaxEditor.GetChangedRoot());
            return newDocument;
        }
    }
}