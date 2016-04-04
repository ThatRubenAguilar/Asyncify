using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Asyncify.Test.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace Asyncify.Test
{
    public class AwaitTaskFixVerifier<TAnalyzer, TProvider> : CodeFixVerifier
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TProvider : CodeFixProvider, new()
    {
        // NOTE: Indent code using this to 2 tabs or face the wrath of the whitespace formatter
        protected static readonly string FullTriviaText = @"// One Line Comment 
#if Directive
        /* Comment If */
#else
        /* Comment Else */
#endif
        /*
        Multi Line Comment 
        */
        #region Region
        #endregion
";

        protected static readonly string TaskChildClass = @"
    using System;
    using System.Threading.Tasks;

    class TaskChild : Task
    {
        public TaskChild() : base(() => { })
        {
        
        }
    }

    class TaskChild<T> : Task<T>
    {
        public TaskChild() : base(() => default(T))
        {
        
        }
    
    }
";

        protected static readonly string TaskStaticClass = @"
    using System;
    using System.Threading.Tasks;

    static class AsyncMethods
    {
        public static TaskChild<int> GetNumber()
        {
            return new TaskChild<int>();
        }

        public static TaskChild PerformProcessing()
        {
            return new TaskChild();
        }

        public static TaskChild<AsyncMemberMethods> GetMemberMethods()
        {
            return new TaskChild<AsyncMemberMethods>();
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
        public Task Result = null;

        public TaskAwaiter<AsyncMemberMethods> GetAwaiter() 
        {
            return null;
        }
        
        public AsyncMemberMethods GetResult() 
        {
            return null;
        }

        public void Wait() 
        {

        }

        public TaskChild<int> GetNumber()
        {
            return new TaskChild<int>();
        }

        public TaskChild PerformProcessing()
        {
            return new TaskChild();
        }
        
        }
";


        protected const int TaskExpressionWrapperStartCol = 0;
        protected const int TaskExpressionWrapperStartLine = 9;
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
        

        protected void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected,
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] {testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass }, expected);

            var fixTaskClass = String.Format(TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new[] {TaskStaticClass, TaskMemberClass, TaskChildClass }, allowNewCompilerDiagnostics:allowNewCompilerDiagnostics);
        }


        protected DiagnosticResult AwaitTaskExpectedResult(string testExpression, string callerTaskExpression, string blockingCallCode, DiagnosticDescriptor rule)
        {
            var lineColOffset = testExpression.FindLineAndColOffset(blockingCallCode);
            var lineLocation = TaskExpressionWrapperStartLine + lineColOffset.Item1;
            var colLocation = TaskExpressionWrapperStartCol + lineColOffset.Item2;
            var expected = new DiagnosticResult
            {
                Id = rule.Id,
                Message = String.Format(rule.MessageFormat.ToString(),
                    callerTaskExpression),
                Severity = rule.DefaultSeverity,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", lineLocation, colLocation)
                    }
            };
            return expected;
        }

    }
}