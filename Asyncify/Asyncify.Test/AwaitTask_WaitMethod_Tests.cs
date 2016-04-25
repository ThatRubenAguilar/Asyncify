using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Asyncify.Test.Extensions;
using Asyncify.Test.Helpers.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_WaitMethod_Tests :
        AwaitTaskFixVerifier
            <ConsiderAwaitOverBlockingTaskWaitAnalyzer, ConsiderAwaitOverBlockingTaskWaitCodeFixProvider>
    {
        private DiagnosticResult AwaitTaskWaitMethodExpectedResult(string testExpression, string callerTaskExpression)
        {
            return AwaitTaskExpectedResults(TaskWrapperCode.MergeCode(testExpression), AsyncifyRules.AwaitTaskWaitRule, "Wait()", callerTaskExpression).Single();
        }
        private IEnumerable<DiagnosticResult> AwaitTaskWaitMethodExpectedResults(string testExpression, params string[] callerTaskExpressions)
        {
            return AwaitTaskExpectedResults(TaskWrapperCode.MergeCode(testExpression), AsyncifyRules.AwaitTaskWaitRule, "Wait()", callerTaskExpressions);
        }

        private void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected, string fixExpression, bool allowNewCompilerDiagnostics = false)
        {
            AwaitTaskDiagnosticAndFix(TaskWrapperCode.MergeCode(testExpression), expected, TaskWrapperCode.MergeCode(fixExpression), allowNewCompilerDiagnostics);
        }

        private void AwaitTaskDiagnosticsAndFix(string testExpression, DiagnosticResult[] expected, string fixExpression, bool allowNewCompilerDiagnostics = false)
        {
            AwaitTaskDiagnosticsAndFix(TaskWrapperCode.MergeCode(testExpression), expected, TaskWrapperCode.MergeCode(fixExpression), allowNewCompilerDiagnostics);
        }

        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }
        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_locked_code()
        {
            var testExpression = @"
lock(this) 
{
    AsyncMethods.PerformProcessing().Wait();
}
";
            var testTaskClass = TaskWrapperCode.MergeCode(testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }
        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_unsafe_code()
        {
            var testExpression = @"
unsafe 
{
    AsyncMethods.PerformProcessing().Wait();
}
";
            var testTaskClass = TaskWrapperCode.MergeCode(testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }
        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_unsafe_method_code()
        {
            var testExpression = @"AsyncMethods.PerformProcessing().Wait();";
            var testMethod = @"unsafe async Task TestMethod()";
            var testTaskClass = TaskMethodWrapperCode.MergeCode(testMethod, testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }
        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_out_method_code()
        {
            var testExpression = @"test = null;
AsyncMethods.PerformProcessing().Wait();";
            var testMethod = @"async Task TestMethod(out AsyncMemberMethods test)";
            var testTaskClass = TaskMethodWrapperCode.MergeCode(testMethod, testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }
        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_ref_method_code()
        {
            var testExpression = @"AsyncMethods.PerformProcessing().Wait();";
            var testMethod = @"unsafe async Task TestMethod(ref AsyncMemberMethods test)";
            var testTaskClass = TaskMethodWrapperCode.MergeCode(testMethod, testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_await_task_fix_on_non_generic_task_wait_method()
        {
            var testExpression = @"
AsyncMethods.PerformProcessing().Wait();";
            var fixExpression = @"
        await AsyncMethods.PerformProcessing();";
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_non_task_wait_method()
        {
            var testExpression = @"(new AsyncMemberMethods()).Wait();";

            var testTaskClass = TaskWrapperCode.MergeCode( testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }

        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_await_diagnostic_for_generic_task_wait_method()
        {
            var testExpression = @"AsyncMethods.GetNumber().Wait();";

            var testTaskClass = TaskWrapperCode.MergeCode( testExpression);

            var expected = AwaitTaskExpectedResult(testTaskClass, AsyncifyRules.RemoveGenericTaskWaitRule, "Wait()", "AsyncMethods.GetNumber()");

            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_keep_trivia_in_await_task_fix_on_non_generic_task_wait_method()
        {
            var testExpression = $@"
        {TestSourceCode.FullTriviaText}
        AsyncMethods.PerformProcessing({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.{TestSourceCode.FullTriviaText}
        Wait(){TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";


            var unformattedTrivia = FullTriviaCode.TriviaTextUniform();
            var formattedTrivia = FullTriviaCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        {unformattedTrivia}
        await AsyncMethods.PerformProcessing({unformattedTrivia}
        ){formattedTrivia}{formattedTrivia}
{formattedTrivia}; {unformattedTrivia}";
            
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_await_task_fix_on_non_generic_task_wait_method_when_using_broken_syntax()
        {
            var testExpression = @"
AsyncMethods.PerformProcessing().Wait()";
            var fixExpression = @"
        await AsyncMethods.PerformProcessing()";
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_await_task_fix_on_non_generic_task_wait_method_when_using_broken_parenthesis_syntax()
        {
            var testExpression = @"(AsyncMethods.PerformProcessing().Wait())";
            var fixExpression = @"        (await AsyncMethods.PerformProcessing())";
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }
    }
}