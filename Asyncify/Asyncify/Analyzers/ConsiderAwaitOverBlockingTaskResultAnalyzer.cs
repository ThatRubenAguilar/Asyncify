using System.Collections.Immutable;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Asyncify.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsiderAwaitOverBlockingTaskResultAnalyzer : DiagnosticAnalyzer
    {
        /*
        NOTE: Analyzers are compact into as few classes as possible to minimize the amount of time rechecking for initial constraints like IsNonUserOrGeneratedCode. CodeFixProviders are organized according to fixes as to maintain sanity.
            */
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(AsyncifyRules.Rules[AsyncifyRules.AwaitTaskResultDiagnosticId], AsyncifyRules.Rules[AsyncifyRules.AwaitTaskGetResultDiagnosticId]); } }

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IdentifierName);
            
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // I don't care about generated code.
            if (context.IsGeneratedOrNonUserCode())
            {
                return;
            }

            var identifierNameSyntax = (IdentifierNameSyntax)context.Node;

            // Name check for Result Task property
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.ResultProperty))
            {
                AnalyzeResultProperty(context, identifierNameSyntax);
            }
            // Name check for GetResult TaskAwaiter method
            else if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.GetResultMethod))
            {
                AnalyzeGetResultMethod(context, identifierNameSyntax);
            }

        }

        private static void AnalyzeResultProperty(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);

            var propertySymbol = symbolInfo.Symbol as IPropertySymbol;

            // Ensure we have a property
            // Ensure we have the Threading.Task
            if (propertySymbol?.ContainingType == null || !AsyncifyResources.TaskRegex.IsMatch(propertySymbol.ContainingType.ToString()))
            {
                return;
            }
            
            var parentTaskName = identifierNameSyntax.Parent.ToStringWithoutTrivia();

            // Trim ".Result"
            var trimmedParentTaskName = parentTaskName.Substring(0,
                parentTaskName.Length - (AsyncifyResources.ResultProperty.Length + 1));

            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(AsyncifyRules.Rules[AsyncifyRules.AwaitTaskResultDiagnosticId], identifierNameSyntax.GetLocation(), trimmedParentTaskName);

            context.ReportDiagnostic(diagnostic);
        }


        private static void AnalyzeGetResultMethod(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);

            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            // Ensure we have a method
            // Ensure we have the Threading.Task
            if (methodSymbol?.ContainingType == null || !AsyncifyResources.TaskAwaiterRegex.IsMatch(methodSymbol.ContainingType.ToString()))
            {
                return;
            }

            // IDEA: Detect and allow fixes off of TaskAwaiter as well, would need to trace the source location of the awaiter through different levels of code.
            var parentTaskAwaiterName = identifierNameSyntax.Parent.ToStringWithoutTrivia();
            // Trim ".GetAwaiter().GetResult", all trivia is stripped so it is normalized to this.
            var trimmedParentTaskAwaiterName = parentTaskAwaiterName.Substring(0,
                parentTaskAwaiterName.Length - (AsyncifyResources.GetAwaiterMethod.Length + AsyncifyResources.GetResultMethod.Length + 4));
            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(AsyncifyRules.Rules[AsyncifyRules.AwaitTaskGetResultDiagnosticId], identifierNameSyntax.GetLocation(), trimmedParentTaskAwaiterName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
