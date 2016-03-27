using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Asyncify
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsiderAwaitOverResultAnalyzer : DiagnosticAnalyzer
    {

        public const string DiagnosticId = "Asyncify";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AsyncifyResultTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AsyncifyResultMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AsyncifyResultDescription), Resources.ResourceManager, typeof(Resources));
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
            
            var parentTaskName = identifierNameSyntax.Parent.ToString();
            // Trim ".Result"
            var trimmedParentTaskName = parentTaskName.Substring(0,
                parentTaskName.Length - (AsyncifyResources.ResultProperty.Length + 1));

            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(Rule, identifierNameSyntax.GetLocation(), trimmedParentTaskName);

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


            var parentTaskAwaiterName = identifierNameSyntax.Parent.ToString();
            // Trim ".GetResult"
            var trimmedParentTaskAwaiterName = parentTaskAwaiterName.Substring(0,
                parentTaskAwaiterName.Length - (AsyncifyResources.GetResultMethod.Length + 1));
            // Save the threads! use await!
            var diagnostic = Diagnostic.Create(Rule, identifierNameSyntax.GetLocation(), trimmedParentTaskAwaiterName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
