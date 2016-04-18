using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Analyzers;
using Asyncify.FixProviders;
using Asyncify.Test.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_GetResultMethod_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverBlockingTaskResultAnalyzer, ConsiderAwaitOverBlockingTaskGetResultCodeFixProvider>
    {

        private DiagnosticResult AwaitTaskGetResultMethodExpectedResult(string testExpression, string callerTaskExpression)
        {
            return AwaitTaskExpectedResult(testExpression, AsyncifyRules.AwaitTaskGetResultRule, "GetResult()", callerTaskExpression);
        }
        private IEnumerable<DiagnosticResult> AwaitTaskGetResultMethodExpectedResults(string testExpression, params string[] callerTaskExpressions)
        {
            return AwaitTaskExpectedResults(testExpression, AsyncifyRules.AwaitTaskGetResultRule, "GetResult()", callerTaskExpressions);
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
            var fixExpression = @"        var val = await AsyncMethods.GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetNumber()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_await_correct_task_fix_on_nested_generic_task_getresult_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().GetNumber().GetAwaiter().GetResult().ToString();";
            var fixOuterExpression = @"        var val = (await AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().GetNumber()).ToString();";
            var fixInnerExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber().GetAwaiter().GetResult().ToString();";
            var fixBothExpression = @"        var val = (await (await AsyncMethods.GetMemberMethods()).GetNumber()).ToString();";
            var expected = AwaitTaskGetResultMethodExpectedResults(testExpression, "AsyncMethods.GetMemberMethods()", "AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().GetNumber()").ToArray();
            
            AwaitTaskDiagnosticsAndFix(testExpression, expected, fixBothExpression);
            AwaitTaskDiagnosticAndFix(testExpression, expected.First(), fixInnerExpression, true);
            AwaitTaskDiagnosticAndFix(testExpression, expected.Last(), fixOuterExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_have_no_diagnostic_for_non_task_getresult_method()
        {
            var testExpression = @"var val = (new AsyncMemberMethods()).GetAwaiter().GetResult();";

            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass });
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_have_no_diagnostic_for_non_direct_task_getresult_method()
        {
            var testExpression = @"var awaiter = (new AsyncMemberMethods()).GetAwaiter();
var val = awaiter.GetResult();";

            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] { testTaskClass, TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass });
        }
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_return_value_not_used()
        {
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
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        GetAwaiter(){TestSourceCode.FullTriviaText}
        .GetResult(){TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        await AsyncMethods.GetMemberMethods({unformattedTrivia}
        ){formattedTrivia}
{formattedTrivia}
{formattedTrivia}
{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }



        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_broken_syntax_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Fi;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Fi;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_broken_syntax()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()";
            var fixExpression = @"        var val = await AsyncMethods.GetMemberMethods()";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_non_generic_task_getresult_method()
        {
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
            var fixExpression = @"        (await AsyncMethods.PerformProcessing())";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression, true);
        }


        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_not_adding_parenthesis_in_await_task_fix_on_non_generic_task_getresult_method()
        {
            var testExpression = $@"
        {TestSourceCode.FullTriviaText}
        AsyncMethods.PerformProcessing({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        GetAwaiter(){TestSourceCode.FullTriviaText}.GetResult(){TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        {unformattedTrivia}
        await AsyncMethods.PerformProcessing({unformattedTrivia}
        ){formattedTrivia}
{formattedTrivia}
{formattedTrivia}{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.PerformProcessing()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        #region Task.GetAwaiter().GetResult().Field

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Field1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).Field1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Field1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }


        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_field()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        GetAwaiter(){TestSourceCode.FullTriviaText}
        .GetResult(){TestSourceCode.FullTriviaText}
        .Field1{TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .Field1{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.GetAwaiter().GetResult().Property
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().Property1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).Property1;";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).Property1;";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_property()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        GetAwaiter(){TestSourceCode.FullTriviaText}
        .GetResult(){TestSourceCode.FullTriviaText}
        .Property1{TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .Property1{formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion

        #region Task.GetAwaiter().GetResult().Method()
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().GetAwaiter().GetResult().GetNumber();";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().GetAwaiter().GetResult()).GetNumber();";
            var fixExpression = @"        var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        
        [TestMethod, TestCategory("Await.Task.GetAwaiter().GetResult()")]
        public void Should_keep_trivia_when_adding_parenthesis_in_await_task_fix_on_generic_task_getresult_method_when_using_task_method()
        {
            var testExpression = $@"
        var val = {TestSourceCode.FullTriviaText}
        AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}
        ){TestSourceCode.FullTriviaText}.
        {TestSourceCode.FullTriviaText}
        GetAwaiter(){TestSourceCode.FullTriviaText}
        .GetResult(){TestSourceCode.FullTriviaText}
        .GetNumber(){TestSourceCode.FullTriviaText}; {TestSourceCode.FullTriviaText}";

            var unformattedTrivia = TestSourceCode.TriviaTextUniform();
            var formattedTrivia = TestSourceCode.TriviaTextFormatted(TestSourceCode.DefaultIndents, TestSourceCode.DefaultIndents);
            var fixExpression = $@"
        var val = {unformattedTrivia}
        (await AsyncMethods.GetMemberMethods({unformattedTrivia}
        )){unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        {unformattedTrivia}
        .GetNumber(){formattedTrivia}; {unformattedTrivia}";
            var expected = AwaitTaskGetResultMethodExpectedResult(testExpression, "AsyncMethods.GetMemberMethods()");

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }
        #endregion
    }
}