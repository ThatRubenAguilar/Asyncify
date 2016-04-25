using System;
using System.Collections.Generic;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Asyncify.Test.Helpers.Code;
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
            return AwaitTaskExpectedResult(TaskWrapperCode.MergeCode(testExpression), AsyncifyRules.RemoveGenericTaskWaitRule, "Wait()", callerTaskExpression);
        }
        private IEnumerable<DiagnosticResult> RemoveTaskWaitMethodExpectedResults(string testExpression, params string[] callerTaskExpressions)
        {
            return AwaitTaskExpectedResults(TaskWrapperCode.MergeCode(testExpression), AsyncifyRules.RemoveGenericTaskWaitRule, "Wait()", callerTaskExpressions);
        }


        private void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected, string fixExpression, bool allowNewCompilerDiagnostics = false)
        {
            AwaitTaskDiagnosticAndFix(TaskWrapperCode.MergeCode(testExpression), expected, TaskWrapperCode.MergeCode(fixExpression), allowNewCompilerDiagnostics);
        }

        private void AwaitTaskDiagnosticsAndFix(string testExpression, DiagnosticResult[] expected, string fixExpression, bool allowNewCompilerDiagnostics = false)
        {
            AwaitTaskDiagnosticsAndFix(TaskWrapperCode.MergeCode(testExpression), expected, TaskWrapperCode.MergeCode(fixExpression), allowNewCompilerDiagnostics);
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

            var testTaskClass = TaskWrapperCode.MergeCode( testExpression);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }

        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_have_no_remove_diagnostic_for_non_generic_task_wait_method()
        {
            var testExpression = @"AsyncMethods.PerformProcessing().Wait();";

            var testTaskClass = TaskWrapperCode.MergeCode( testExpression);

            var expected = AwaitTaskExpectedResult(testTaskClass, AsyncifyRules.AwaitTaskWaitRule, "Wait()", "AsyncMethods.PerformProcessing()");

            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);
        }


        [TestMethod, TestCategory("Remove.Task.Wait()")]
        public void Should_keep_trivia_in_remove_task_fix_on_generic_task_wait_method()
        {
            var testExpression = $@"
        {TestSourceCode.FullTriviaText}
        AsyncMethods.GetNumber({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        Wait(){TestSourceCode.FullTriviaText};
        {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = FullTriviaCode.TriviaTextUniform();
            var fixExpression = $@"
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}";
            
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