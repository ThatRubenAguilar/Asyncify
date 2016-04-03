using System.Collections.Generic;
using System.Collections.ObjectModel;
using Asyncify.Analyzers;
using Microsoft.CodeAnalysis;

namespace Asyncify
{
    public static class AsyncifyRules
    {
        public static DiagnosticDescriptor AwaitTaskResultRule { get; }
        public static DiagnosticDescriptor AwaitTaskGetResultRule { get; }
        public static DiagnosticDescriptor AwaitTaskWaitRule { get; }
        public static DiagnosticDescriptor RemoveGenericTaskWaitRule { get; }

        static AsyncifyRules()
        {
            AwaitTaskResultRule = CreateAwaitTaskResultRule();
            AwaitTaskGetResultRule = CreateAwaitTaskGetResultRule();
            AwaitTaskWaitRule = CreateAwaitTaskWaitRule();
            RemoveGenericTaskWaitRule = CreateRemoveGenericTaskWaitRule();
        }
        
        private static DiagnosticDescriptor CreateAwaitTaskResultRule()
        {
            string DiagnosticId = AsyncifyDiagnosticIds.AwaitTaskResultDiagnosticId;

            LocalizableString title = new LocalizableResourceString(nameof(Resources.ResultTitle),
                Resources.ResourceManager, typeof (Resources));
            LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.ResultMessageFormat),
                Resources.ResourceManager, typeof (Resources));
            LocalizableString description = new LocalizableResourceString(nameof(Resources.ResultDescription),
                Resources.ResourceManager, typeof (Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category,
                DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

            return rule;
        }
        private static DiagnosticDescriptor CreateAwaitTaskGetResultRule()
        {
            string DiagnosticId = AsyncifyDiagnosticIds.AwaitTaskGetResultDiagnosticId;

            LocalizableString title = new LocalizableResourceString(nameof(Resources.GetResultTitle), Resources.ResourceManager, typeof(Resources));
            LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.GetResultMessageFormat), Resources.ResourceManager, typeof(Resources));
            LocalizableString description = new LocalizableResourceString(nameof(Resources.GetResultDescription), Resources.ResourceManager, typeof(Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

            return rule;
        }
        private static DiagnosticDescriptor CreateAwaitTaskWaitRule()
        {
            string DiagnosticId = AsyncifyDiagnosticIds.AwaitTaskWaitDiagnosticId;

            LocalizableString title = new LocalizableResourceString(nameof(Resources.WaitTitle), Resources.ResourceManager, typeof(Resources));
            LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.WaitMessageFormat), Resources.ResourceManager, typeof(Resources));
            LocalizableString description = new LocalizableResourceString(nameof(Resources.WaitDescription), Resources.ResourceManager, typeof(Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

            return rule;
        }
        private static DiagnosticDescriptor CreateRemoveGenericTaskWaitRule()
        {
            string DiagnosticId = AsyncifyDiagnosticIds.RemoveGenericTaskWaitDiagnosticId;

            LocalizableString title = new LocalizableResourceString(nameof(Resources.RemoveWaitTitle), Resources.ResourceManager, typeof(Resources));
            LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.RemoveWaitMessageFormat), Resources.ResourceManager, typeof(Resources));
            LocalizableString description = new LocalizableResourceString(nameof(Resources.RemoveWaitDescription), Resources.ResourceManager, typeof(Resources));
            string Category = "Refactoring";

            DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

            return rule;
        }
    }
}