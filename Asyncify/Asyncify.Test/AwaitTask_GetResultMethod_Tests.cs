using System;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_GetResultMethod_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverBlockingTaskResultAnalyzer, ConsiderAwaitOverBlockingTaskGetResultCodeFixProvider>
    {

        private DiagnosticResult AwaitTaskGetResultMethodExpectedResult(string testExpression, string resultCaller)
        {
            var lineColOffset = FindLineAndColOffset(testExpression, "GetResult()");
            var lineLocation = TaskExpressionWrapperStartLine + lineColOffset.Item1;
            var colLocation = TaskExpressionWrapperStartCol + lineColOffset.Item2;
            var rule = AsyncifyRules.AwaitTaskGetResultRule;
            var expected = new DiagnosticResult
            {
                Id = rule.Id,
                Message = String.Format(rule.MessageFormat.ToString(),
                    resultCaller),
                Severity = rule.DefaultSeverity,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", lineLocation, colLocation)
                    }
            };
            return expected;
        }
        

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_await_task_fix_on_generic_task_getresult_method()
        {
            var testExpression = @"var val = AsyncMethods.GetNumber().GetAwaiter().GetResult();";
            var fixExpression = @"var val = await AsyncMethods.GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_have_no_diagnostic_for_non_task_getresult_method()
        {
            var testExpression = @"var val = (new AsyncMemberMethods()).GetAwaiter().GetResult();";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass });
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_have_no_diagnostic_for_non_direct_task_getresult_method()
        {
            var testExpression = @"var awaiter = (new AsyncMemberMethods()).GetAwaiter();
var val = awaiter.GetResult();";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass, TaskChildClass });
        }
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_return_value_not_used()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
            var testExpression = @"
AsyncMethods.GetMemberMethods().GetAwaiter().GetResult();";
            var fixExpression = @"
        await AsyncMethods.GetMemberMethods();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_not_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method()
        {
            var testExpression = @"
        var val = /*please*/
        #region do
        #endregion
        AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        )/*bah*/.// who
        #region even
            GetAwaiter()/*doubles*/.GetResult(); // comments
        #endregion";
            var fixExpression = @"
        var val = /*please*/
        #region do
        #endregion
        await AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        )/*bah*/// who
        #region even
            /*doubles*/; // comments
        #endregion";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }



        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_broken_syntax_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Fi;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Fi;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_broken_syntax()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()";
            var fixExpression = @"var val = await AsyncMethods.GetMemberMethods()";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_non_generic_task_getresult_method()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
            var testExpression = @"
AsyncMethods.PerformProcessing().GetAwaiter().GetResult();";
            var fixExpression = @"
        await AsyncMethods.PerformProcessing();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_non_generic_task_getresult_method_when_using_broken_syntax()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
            var testExpression = @"
AsyncMethods.PerformProcessing().GetAwaiter().GetResult()";
            var fixExpression = @"
        await AsyncMethods.PerformProcessing()";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_non_generic_task_getresult_method_when_using_broken_parenthesis_syntax()
        {
            var testExpression = @"(AsyncMethods.PerformProcessing().GetAwaiter().GetResult())";
            var fixExpression = @"(await AsyncMethods.PerformProcessing())";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }


        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_not_adding_parenthesis_in_await_task_fix_on_non_generic_task_getresult_method()
        {
            var testExpression = @"
        /*please*/
        #region do
        #endregion
        AsyncMethods.PerformProcessing(
        #region stop
        #endregion
        )/*bah*/.// who
        #region even
            GetAwaiter()/*doubles*/.GetResult(); // comments
        #endregion";
            var fixExpression = @"
        /*please*/
        #region do
        #endregion
        await AsyncMethods.PerformProcessing(
        #region stop
        #endregion
        )/*bah*/// who
        #region even
            /*doubles*/; // comments
        #endregion";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        #region Task.GetAwaiter().GetResult().Field

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = @"
        var val = /*please*/
        #region do
        #endregion
        AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        )/*bah*/.// who
        #region even
            GetAwaiter()/*doubles*/.GetResult() // comments
            .Field1;
        #endregion";
            var fixExpression = @"
        var val = /*please*/
        #region do
        #endregion
        (await AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        ))/*bah*/// who
        #region even
            /*doubles*/ // comments
            .Field1;
        #endregion";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.GetAwaiter().GetResult().Property
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = @"
        var val = /*please*/
        #region do
        #endregion
        AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        )/*bah*/.// who
        #region even
            GetAwaiter()/*doubles*/.GetResult() // comments
            .Property1;
        #endregion";
            var fixExpression = @"
        var val = /*please*/
        #region do
        #endregion
        (await AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        ))/*bah*/// who
        #region even
            /*doubles*/ // comments
            .Property1;
        #endregion";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.GetAwaiter().GetResult().Method()
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = @"
        var val = /*please*/
        #region do
        #endregion
        AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        )/*bah*/.// who
        #region even
            GetAwaiter()/*doubles*/.GetResult() // comments
            .GetNumber();
        #endregion";
            var fixExpression = @"
        var val = /*please*/
        #region do
        #endregion
        (await AsyncMethods.GetMemberMethods(
        #region stop
        #endregion
        ))/*bah*/// who
        #region even
            /*doubles*/ // comments
            .GetNumber();
        #endregion";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion
    }
}