using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_ResultProperty_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverBlockingTaskResultAnalyzer, ConsiderAwaitOverBlockingTaskResultCodeFixProvider>
    {

        private DiagnosticResult AwaitTaskResultPropertyExpectedResult(string testExpression, string resultCaller)
        {
            var lineColOffset = FindLineAndColOffset(testExpression, "Result");
            var lineLocation = TaskExpressionWrapperStartLine + lineColOffset.Item1;
            var colLocation = TaskExpressionWrapperStartCol + lineColOffset.Item2;
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverBlockingTaskResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverBlockingTaskResultAnalyzer.MessageFormat.ToString(),
                    resultCaller),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", lineLocation, colLocation)
                    }
            };
            return expected;
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_await_task_fix_on_generic_task_result_property()
        {
            var testExpression = @"var val = AsyncMethods.GetNumber().Result;";
            var fixExpression = @"var val = await AsyncMethods.GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_have_no_diagnostic_for_non_task_result_property()
        {
            var testExpression = @"var val = (new AsyncMemberMethods()).Result;";

            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TaskStaticClass, TaskMemberClass });
        }
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_return_value_not_used()
        {
            // BUG: The document emitted by the code fix provider does not have two idents, but the document in the changed solution after the fix is applied has two tab (4 spaces) idents on the fixed expression. This seems to only occur with raw expression calls. 
            var testExpression = @"
AsyncMethods.GetMemberMethods().Result;";
            var fixExpression = @"
        await AsyncMethods.GetMemberMethods();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_not_adding_parenthesis_in_await_task_fix_on_generic_task_result_property()
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
            Result; // comments
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
            ; // comments
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_broken_syntax_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Fi;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Fi;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_broken_syntax()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result";
            var fixExpression = @"var val = await AsyncMethods.GetMemberMethods()";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        #region Task.Result.Field

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_field()
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
            Result // comments
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
            // comments
            .Field1;
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.Result.Property
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_property()
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
            Result // comments
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
            // comments
            .Property1;
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.Result.Method()
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_method()
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
            Result // comments
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
            // comments
            .GetNumber();
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
#endregion
    }
}