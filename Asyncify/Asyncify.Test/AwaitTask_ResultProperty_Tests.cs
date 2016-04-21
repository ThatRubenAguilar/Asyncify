using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Asyncify.Test.Extensions;
using TestHelper;

namespace Asyncify.Test
{
    // TODO: Add lock/unsafe/out/ref tests
    [TestClass]
    public class AwaitTask_ResultProperty_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverBlockingTaskResultAnalyzer, ConsiderAwaitOverBlockingTaskResultCodeFixProvider>
    {

        private DiagnosticResult AwaitTaskResultPropertyExpectedResult(string testExpression, string callerTaskExpression)
        {
            return AwaitTaskExpectedResult(testExpression, AsyncifyRules.AwaitTaskResultRule, "Result", callerTaskExpression);
        }
        private IEnumerable<DiagnosticResult> AwaitTaskResultPropertyExpectedResults(string testExpression, params string[] callerTaskExpressions)
        {
            return AwaitTaskExpectedResults(testExpression, AsyncifyRules.AwaitTaskResultRule, "Result", callerTaskExpressions);
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
            var fixExpression = @"        var val = await AsyncMethods.GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_await_correct_task_fix_on_nested_generic_task_result_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.GetNumber().Result.ToString();";
            var fixOuterExpression = @"        var val = (await AsyncMethods.GetMemberMethods().Result.GetNumber()).ToString();";
            var fixInnerExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber().Result.ToString();";
            var fixBothExpression = @"        var val = (await (await AsyncMethods.GetMemberMethods()).GetNumber()).ToString();";
            var expected = AwaitTaskResultPropertyExpectedResults(testExpression, "AsyncMethods.GetMemberMethods()", "AsyncMethods.GetMemberMethods().Result.GetNumber()").ToArray();

            AwaitTaskDiagnosticsAndFix(testExpression, expected, fixBothExpression);
            AwaitTaskDiagnosticAndFix(testExpression, expected.First(), fixInnerExpression, true);
            AwaitTaskDiagnosticAndFix(testExpression, expected.Last(), fixOuterExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_have_no_diagnostic_for_non_task_result_property()
        {
            var testExpression = @"var val = (new AsyncMemberMethods()).Result;";

            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(TestSourceCode.GetCompilationSources(testTaskClass));
        }
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_return_value_not_used()
        {
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
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.{TestSourceCode.FullTriviaText}
        Result{TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        await AsyncMethods.GetMemberMethods({unformattedTrivia}
        ){formattedTrivia}{formattedTrivia}
{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_broken_syntax_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Fi;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Fi;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_broken_syntax()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result";
            var fixExpression = @"        var val = await AsyncMethods.GetMemberMethods()";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        #region Task.Result.Field

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Field1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Field1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_field()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        Result{TestSourceCode.FullTriviaText}
        .Field1{TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}
";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .Field1{formattedTrivia}; {unformattedTrivia}
";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.Result.Property
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Property1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Property1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_property()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        Result {TestSourceCode.FullTriviaText}
        .Property1{TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .Property1{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.Result.Method()
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.GetNumber();";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).GetNumber();";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        
        [TestMethod, TestCategory("Await.Task.Result")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        Result{TestSourceCode.FullTriviaText}
        .GetNumber(){TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .GetNumber(){formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskResultPropertyExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
#endregion
    }
}