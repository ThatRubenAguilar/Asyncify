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
    public class ConsiderAwaitOverBlockingTaskResultAnalyzer : DiagnosticAnalyzer
    {
        /*
        NOTE: Analyzers are compact into as few classes as possible to minimize the amount of time rechecking for initial constraints like IsNonUserOrGeneratedCode. CodeFixProviders are organized according to fixes as to maintain sanity.
            */
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(AsyncifyRules.AwaitTaskResultRule, AsyncifyRules.AwaitTaskGetResultRule); } }

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
            
            AnalyzeResultProperty(context, identifierNameSyntax);
            AnalyzeGetResultMethod(context, identifierNameSyntax);
        }

        private static void AnalyzeResultProperty(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            // Name check for Result Task property
            if (!identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.ResultProperty))
                return;
            
            var propertySymbol = context.SemanticModel.GetSymbol<IPropertySymbol>(identifierNameSyntax, context.CancellationToken);

            // Ensure we have a property
            // Ensure we have the Threading.Task
            if (propertySymbol?.ContainingType == null || !AsyncifyResources.TaskRegex.IsMatch(propertySymbol.ContainingType.ToString()))
            {
                return;
            }


            // IDEA: In C# <6.0 await is also not allowed in catch/finally blocks, could filter for that once we figure out which version VS solution is targetting, see: http://stackoverflow.com/questions/247621/what-are-the-correct-version-numbers-for-c/247623#247623 and http://csharpindepth.com/Articles/Chapter1/Versions.aspx and potential solution http://geekswithblogs.net/lorint/archive/2006/01/30/67654.aspx
            // Not within lock or unsafe context
            if (identifierNameSyntax.ContainedWithin<LockStatementSyntax>() || identifierNameSyntax.ContainedWithin<UnsafeStatementSyntax>())
            {
                return;
            }

            var containingMethod = identifierNameSyntax.ContainedWithinNodeOrDefault<MethodDeclarationSyntax>();
            // Containing method does not have ref or out params or method has unsafe context
            if (containingMethod != null && (containingMethod.ContainsParameterModifiers(SyntaxKind.RefKeyword, SyntaxKind.OutKeyword) || containingMethod.Modifiers.Any(m => m.IsKind(SyntaxKind.UnsafeKeyword))))
            {
                return;
            }

            var parentTaskName = identifierNameSyntax.Parent.ToStringWithoutTrivia();

            // Trim ".Result"
            var trimmedParentTaskName = parentTaskName.Substring(0,
                parentTaskName.Length - (AsyncifyResources.ResultProperty.Length + 1));

            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(AsyncifyRules.AwaitTaskResultRule, identifierNameSyntax.GetLocation(), trimmedParentTaskName);

            context.ReportDiagnostic(diagnostic);
        }


        private static void AnalyzeGetResultMethod(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            // Name check for GetResult TaskAwaiter method
            if (!identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.GetResultMethod))
                return;
            
            var methodSymbol = context.SemanticModel.GetSymbol<IMethodSymbol>(identifierNameSyntax, context.CancellationToken);

            // Ensure we have a method
            // Ensure we have the Threading.Task
            if (methodSymbol?.ContainingType == null || !AsyncifyResources.TaskAwaiterRegex.IsMatch(methodSymbol.ContainingType.ToString()))
            {
                return;
            }


            // IDEA: In C# <6.0 await is also not allowed in catch/finally blocks, could filter for that once we figure out which version VS solution is targetting, see: http://stackoverflow.com/questions/247621/what-are-the-correct-version-numbers-for-c/247623#247623 and http://csharpindepth.com/Articles/Chapter1/Versions.aspx and potential solution http://geekswithblogs.net/lorint/archive/2006/01/30/67654.aspx
            // Not within lock or unsafe context
            if (identifierNameSyntax.ContainedWithin<LockStatementSyntax>() || identifierNameSyntax.ContainedWithin<UnsafeStatementSyntax>())
            {
                return;
            }

            var containingMethod = identifierNameSyntax.ContainedWithinNodeOrDefault<MethodDeclarationSyntax>();
            // Containing method does not have ref or out params or method has unsafe context
            if (containingMethod != null && (containingMethod.ContainsParameterModifiers(SyntaxKind.RefKeyword, SyntaxKind.OutKeyword) || containingMethod.Modifiers.Any(m => m.IsKind(SyntaxKind.UnsafeKeyword))))
            {
                return;
            }

            // IDEA: Detect and allow fixes off of TaskAwaiter as well, would need to trace the source location of the awaiter through different levels of code.
            var parentTaskAwaiterName = identifierNameSyntax.Parent.ToStringWithoutTrivia();
            // Trim ".GetAwaiter().GetResult", all trivia is stripped so it is normalized to this.
            var trimmedParentTaskAwaiterName = parentTaskAwaiterName.Substring(0,
                parentTaskAwaiterName.Length - (AsyncifyResources.GetAwaiterMethod.Length + AsyncifyResources.GetResultMethod.Length + 4));
            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(AsyncifyRules.AwaitTaskGetResultRule, identifierNameSyntax.GetLocation(), trimmedParentTaskAwaiterName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
