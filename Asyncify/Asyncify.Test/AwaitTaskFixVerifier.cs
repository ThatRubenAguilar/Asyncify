using System;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace Asyncify.Test
{
    public class AwaitTaskFixVerifier<TAnalyzer, TProvider> : CodeFixVerifier
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TProvider : CodeFixProvider, new()
    {
        protected static readonly string TaskStaticClass = @"
    using System;
    using System.Threading.Tasks;

    static class AsyncMethods
    {
        public static Task<int> GetNumber()
        {
            return Task.FromResult(42);
        }

        public static Task PerformProcessing()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        public static Task<AsyncMemberMethods> GetMemberMethods()
        {
            return Task.FromResult(new AsyncMemberMethods());
        }
    }
";

        protected static readonly string TaskMemberClass = @"
    using System;
    using System.Threading.Tasks;

    class AsyncMemberMethods
    {
        public AsyncMemberMethods Field1 = null;
        public AsyncMemberMethods Property1 => null;

        public Task<int> GetNumber()
        {
            return Task.FromResult(42);
        }

        public Task PerformProcessing()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }
        
    }
";
        // Starts at line 9 and col 9
        protected static readonly string TaskExpressionWrapper = @"
using System;
using System.Threading.Tasks;

class Test
{{
    async Task TestMethod()
    {{
        {0}
    }}
}}
";
        
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TAnalyzer();
        }
        
        protected void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected, string fixedExpression)
        {
            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass }, expected);

            var fixTaskClass = String.Format(TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new []{ TaskStaticClass, TaskMemberClass });
        }
    }
}