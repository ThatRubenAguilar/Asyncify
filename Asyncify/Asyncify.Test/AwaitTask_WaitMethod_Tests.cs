using System;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Asyncify.Test.Extensions;
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
            return AwaitTaskExpectedResult(testExpression, callerTaskExpression, "Wait()", AsyncifyRules.AwaitTaskWaitRule);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_await_task_fix_on_non_generic_task_wait_method()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
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

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass });
        }

        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_have_no_await_diagnostic_for_generic_task_wait_method()
        {
            var testExpression = @"AsyncMethods.GetNumber().Wait();";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);

            var expected = AwaitTaskExpectedResult(testExpression, "AsyncMethods.GetNumber()", "Wait()", AsyncifyRules.RemoveGenericTaskWaitRule);

            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass }, expected);
        }


        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_keep_trivia_in_await_task_fix_on_non_generic_task_wait_method()
        {
            var testExpression = $@"
        {FullTriviaText}
        AsyncMethods.PerformProcessing({FullTriviaText}
        ){FullTriviaText}.{FullTriviaText}
        Wait(){FullTriviaText}; {FullTriviaText}";
            var fixExpression = $@"
        {FullTriviaText}
        await AsyncMethods.PerformProcessing({FullTriviaText}
        ){FullTriviaText}{FullTriviaText}
        {FullTriviaText}; {FullTriviaText}";
            
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Wait()")]
        public void Should_await_task_fix_on_non_generic_task_wait_method_when_using_broken_syntax()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
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
            var fixExpression = @"(await AsyncMethods.PerformProcessing())";
            var expected = AwaitTaskWaitMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }
    }
}