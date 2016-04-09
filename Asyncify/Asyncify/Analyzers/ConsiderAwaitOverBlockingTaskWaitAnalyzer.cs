using System.Collections.Immutable;
using System.Linq;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Asyncify.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsiderAwaitOverBlockingTaskWaitAnalyzer : DiagnosticAnalyzer
    {

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(AsyncifyRules.AwaitTaskWaitRule, AsyncifyRules.RemoveGenericTaskWaitRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
            
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // I don't care about generated code.
            if (context.IsGeneratedOrNonUserCode())
            {
                return;
            }

            var memberAccessExpression = (MemberAccessExpressionSyntax)context.Node;

            var identifierNameSyntax = memberAccessExpression.ChildNodes().OfType<IdentifierNameSyntax>().LastOrDefault();

            if (identifierNameSyntax == null)
                return;

            // Name check for Wait method
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.WaitMethod))
            {
                AnalyzeWaitMethod(context, identifierNameSyntax);
            }

        }
        
        private static void AnalyzeWaitMethod(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);

            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            // Ensure we have a method
            // Ensure we have the Threading.Task
            if (methodSymbol?.ContainingType == null || !AsyncifyResources.TaskRegex.IsMatch(methodSymbol.ContainingType.ToString()))
            {
                return;
            }

            var waitInvoker = identifierNameSyntax.Parent.ChildNodes().First();

            var invokerTypeInfo = context.SemanticModel.GetTypeInfo(waitInvoker, context.CancellationToken);

            bool isGenericTask =
                invokerTypeInfo.ConvertedType.PartialMatchAncestorTypesOrSelf(AsyncifyResources.TaskGenericRegex,
                    AsyncifyResources.TaskRegex);

            // If we have a generic Threading.Task suggest removal instead
            if (isGenericTask)
            {
                AnalyzeTaskWait(context, identifierNameSyntax, AsyncifyRules.RemoveGenericTaskWaitRule);
            }
            // Proceed with await suggestion
            else
            {
                AnalyzeTaskWait(context, identifierNameSyntax, AsyncifyRules.AwaitTaskWaitRule);
            }
        }

        private static void AnalyzeTaskWait(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax, DiagnosticDescriptor rule)
        {
            var parentTaskAwaiterName = identifierNameSyntax.Parent.ToStringWithoutTrivia();
            // Trim ".Wait", all trivia is stripped so it is normalized to this.
            var trimmedParentTaskAwaiterName = parentTaskAwaiterName.Substring(0,
                parentTaskAwaiterName.Length - (AsyncifyResources.WaitMethod.Length + 1));
            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(rule,
                identifierNameSyntax.GetLocation(),
                trimmedParentTaskAwaiterName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}