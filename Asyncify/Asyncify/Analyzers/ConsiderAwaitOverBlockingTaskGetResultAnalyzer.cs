using System.Collections.Immutable;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Asyncify.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsiderAwaitOverBlockingTaskGetResultAnalyzer : DiagnosticAnalyzer
    {

        public const string DiagnosticId = "ASYNC0002";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Refactoring";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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
            
            // Name check for GetResult TaskAwaiter method
            if (identifierNameSyntax.Identifier.Text.Equals(AsyncifyResources.GetResultMethod))
            {
                AnalyzeGetResultMethod(context, identifierNameSyntax);
            }

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
            var diagnostic = Diagnostic.Create(Rule, identifierNameSyntax.GetLocation(), trimmedParentTaskAwaiterName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}