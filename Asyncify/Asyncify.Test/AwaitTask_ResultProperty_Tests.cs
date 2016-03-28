using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_ResultProperty_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverResultAnalyzer, ConsiderAwaitOverResultCodeFixProvider>
    {

        private DiagnosticResult AwaitTaskResultPropertyExpectedResult(string testExpression, string resultCaller)
        {
            var lineColOffset = FindLineAndColOffset(testExpression, "Result");
            var lineLocation = TaskExpressionWrapperStartLine + lineColOffset.Item1;
            var colLocation = TaskExpressionWrapperStartCol + lineColOffset.Item2;
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
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

        [TestMethod]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod]
        public void Should_await_task_fix_on_generic_task_result_property()
        {
            var testExpression = @"var val = AsyncMethods.GetNumber().Result;";
            var fixExpression = @"var val = await AsyncMethods.GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Field1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Property1;";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).GetNumber();";
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        
        [TestMethod]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"
        var val = AsyncMethods.GetMemberMethods()/*bah*/.// who
        #region even
            Result // comments
            .GetNumber();
        #endregion";
            var fixExpression = @"
        var val = (await AsyncMethods.GetMemberMethods())/*bah*/// who
        #region even
            // comments
            .GetNumber();
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        [TestMethod]
        public void Should_keep_trivia_when_not_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"
        var val = AsyncMethods.GetMemberMethods()/*bah*/.// who
        #region even
            Result; // comments
        #endregion";
            var fixExpression = @"
        var val = await AsyncMethods.GetMemberMethods()/*bah*/// who
        #region even
            ; // comments
        #endregion";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
    }
}