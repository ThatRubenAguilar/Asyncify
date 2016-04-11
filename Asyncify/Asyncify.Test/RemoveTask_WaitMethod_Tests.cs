using System;
using System.Collections.Generic;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class RemoveTask_WaitMethod_Tests :
        AwaitTaskFixVerifier
            <ConsiderAwaitOverBlockingTaskWaitAnalyzer, ConsiderRemovingBlockingGenericTaskWaitCodeFixProvider>
    {
        private DiagnosticResult RemoveTaskWaitMethodExpectedResult(string testExpression, string callerTaskExpression)
        {
            return AwaitTaskExpectedResult(testExpression, AsyncifyRules.RemoveGenericTaskWaitRule, "Wait()", callerTaskExpression);
        }
        private IEnumerable<DiagnosticResult> RemoveTaskWaitMethodExpectedResults(string testExpression, params string[] callerTaskExpressions)
        {
            return AwaitTaskExpectedResults(testExpression, AsyncifyRules.RemoveGenericTaskWaitRule, "Wait()", callerTaskExpressions);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_remove_task_fix_on_generic_task_wait_method()
        {
            var testExpression = @"AsyncMethods.GetNumber().Wait();";
            var fixExpression = @"";
            var expected = RemoveTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_have_no_diagnostic_for_non_task_wait_method()
        {
            var testExpression = @"(new AsyncMemberMethods()).Wait();";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass });
        }

        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_have_no_remove_diagnostic_for_non_generic_task_wait_method()
        {
            var testExpression = @"AsyncMethods.PerformProcessing().Wait();";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);

            var expected = AwaitTaskExpectedResult(testExpression, AsyncifyRules.AwaitTaskWaitRule, "Wait()", "AsyncMethods.PerformProcessing()");

            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass }, expected);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_keep_trivia_in_remove_task_fix_on_generic_task_wait_method()
        {
            var testExpression = $@"
        {FullTriviaText}
        AsyncMethods.GetNumber({FullTriviaText}
        ){FullTriviaText}.
        {FullTriviaText}
        Wait(){FullTriviaText};
        {FullTriviaText}";
            var fixExpression = $@"
        {FullTriviaText}
        {FullTriviaText}
        {FullTriviaText}
        {FullTriviaText}
        {FullTriviaText}
        {FullTriviaText}";
            
            var expected = RemoveTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_remove_task_fix_on_generic_task_wait_method_when_using_broken_syntax()
        {
            var testExpression = @"AsyncMethods.GetNumber().Wait()";
            var fixExpression = @"";
            var expected = RemoveTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_remove_task_fix_on_generic_task_wait_method_when_using_broken_parenthesis_syntax()
        {
            var testExpression = @"(AsyncMethods.GetNumber().Wait())";
            var fixExpression = @"";
            var expected = RemoveTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }
    }
}