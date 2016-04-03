using System.Collections.Generic;
using System.Collections.ObjectModel;
using Asyncify.Analyzers;
using Microsoft.CodeAnalysis;

namespace Asyncify
{
    public static class AsyncifyRules
    {
        public const string AwaitTaskResultDiagnosticId = "ASYNC0001";
        public const string AwaitTaskGetResultDiagnosticId = "ASYNC0002";
        public const string AwaitTaskWaitDiagnosticId = "ASYNC0003";
        public const string RemoveGenericTaskWaitDiagnosticId = "ASYNC0004";

        public static readonly ReadOnlyDictionary<string, DiagnosticDescriptor> Rules = new ReadOnlyDictionary<string, DiagnosticDescriptor>(CreateAsyncifyRules());

        private static IDictionary<string, DiagnosticDescriptor> CreateAsyncifyRules()
        {
            var rules = new Dictionary<string, DiagnosticDescriptor>();

            CreateAwaitTaskResultRule(rules);
            CreateAwaitTaskGetResultRule(rules);

            return rules;
        }

        private static void CreateAwaitTaskResultRule(Dictionary<string, DiagnosticDescriptor> rules)
        {
            string DiagnosticId = AwaitTaskResultDiagnosticId;

            LocalizableString Title = new LocalizableResourceString(nameof(Resources.AsyncifyResultTitle),
                Resources.ResourceManager, typeof (Resources));
            LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AsyncifyResultMessageFormat),
                Resources.ResourceManager, typeof (Resources));
            LocalizableString Description = new LocalizableResourceString(nameof(Resources.AsyncifyResultDescription),
                Resources.ResourceManager, typeof (Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

            rules.Add(DiagnosticId, Rule);
        }
        private static void CreateAwaitTaskGetResultRule(Dictionary<string, DiagnosticDescriptor> rules)
        {
            string DiagnosticId = AwaitTaskGetResultDiagnosticId;

            LocalizableString Title = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultTitle), Resources.ResourceManager, typeof(Resources));
            LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultMessageFormat), Resources.ResourceManager, typeof(Resources));
            LocalizableString Description = new LocalizableResourceString(nameof(Resources.AsyncifyGetResultDescription), Resources.ResourceManager, typeof(Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

            rules.Add(DiagnosticId, Rule);
        }
    }
}