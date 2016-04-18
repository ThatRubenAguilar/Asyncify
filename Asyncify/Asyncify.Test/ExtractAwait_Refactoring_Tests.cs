using Asyncify.RefactorProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class ExtractAwait_Refactoring_Tests :
        AwaitTaskRefactoringVerifier<ExtractAwaitExpressionToVariableRefactorProvider>
    {

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_have_no_change_for_empty_code()
        {
            var test = @"";
            AwaitTaskRefactoring(test, null, test);
        }

        #region Code Block
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_block_code_for_generic_task()
        {
            var testExpression = @"
{
    var t = (await AsyncMethods.GetMemberMethods()).Field1;
}
";
            var fixExpression = @"
{
        AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
        var t = (taskResult).Field1;
}
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_block_code_when_generic_task_not_awaited()
        {
            var testExpression = @"
{
    var t = AsyncMethods.GetMemberMethods().Result.Field1;
}
";
            AwaitTaskRefactoring(testExpression, null, testExpression);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_block_code_for_non_generic_task()
        {
            var testExpression = @"
{
    await AsyncMethods.PerformProcessing();
}
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, testExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_commented_block_code_for_generic_task()
        {
            // NOTE: Still unsure how to disable autoformatting, but it will apply whitespace corrections to moved or modified expressions, including trivia.
            var testExpression = $@"
        {{
            var t = ({TestSourceCode.FullTriviaText}await AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}.Field1;
        }}
";
            var correctedTrivia = TestSourceCode.TriviaTextCorrected(TestSourceCode.DefaultIndents + 1);
            var fixExpression = $@"
        {{
            AsyncMemberMethods taskResult =
{correctedTrivia}await AsyncMethods.GetMemberMethods({correctedTrivia}){correctedTrivia};
            var t = (taskResult){correctedTrivia}.Field1;
        }}
";
            // TODO: Complete refactoring now that formatting is involved. Add corrected trivia to comment tests
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_block_code_for_generic_task_on_broken_syntax()
        {
            var testExpression = @"
{
    var t = (await AsyncMethods.GetMemberMethods()).Field1
}
";
            var fixExpression = @"
{
        AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
        var t = (taskResult).Field1
}
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }
        #endregion Code Block
        #region Lambda Block
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_block_code_for_generic_task()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{
    return (await AsyncMethods.GetMemberMethods()).Field1;
};
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1;
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }
        
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_lambda_block_code_when_generic_task_not_awaited()
        {
            var testExpression = @"
Func<AsyncMemberMethods> lambda = () =>
{
    return AsyncMethods.GetMemberMethods().Result.Field1;
};
";
            AwaitTaskRefactoring(testExpression, null, testExpression);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_lambda_block_code_for_non_generic_task()
        {
            var testExpression = @"
Action lambda = async () =>
{
    await AsyncMethods.PerformProcessing();
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, testExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_commented_lambda_block_code_for_generic_task()
        {
            var testExpression = $@"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{{
    return ({TestSourceCode.FullTriviaText}await AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}.Field1;
}};
";
            var whitespaceCorrectedTrivia = TestSourceCode.TriviaTextCustom(TestSourceCode.DefaultIndents);
            var fixExpression = $@"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{{
AsyncMemberMethods taskResult =
{whitespaceCorrectedTrivia}await AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}){whitespaceCorrectedTrivia};
return (taskResult){TestSourceCode.FullTriviaText}.Field1;
}};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_block_code_for_generic_task_on_broken_syntax()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{
    return (await AsyncMethods.GetMemberMethods()).Field1
};
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () =>
{
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_block_code_for_generic_task_with_non_async_broken_syntax()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = () =>
{
    return (await AsyncMethods.GetMemberMethods()).Field1;
};
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = () =>
{
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1;
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression, true);
        }
        #endregion Lambda Block
        #region Lambda Single Line
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_single_line_for_generic_task()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () => (await AsyncMethods.GetMemberMethods()).Field1;
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () => {
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1;
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }
        
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_lambda_single_line_when_generic_task_not_awaited()
        {
            var testExpression = @"
Func<AsyncMemberMethods> lambda = () => AsyncMethods.GetMemberMethods().Result.Field1;
";
            AwaitTaskRefactoring(testExpression, null, testExpression);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_not_extract_await_in_lambda_single_line_for_non_generic_task()
        {
            var testExpression = @"
Action lambda = async () => await AsyncMethods.PerformProcessing();
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, testExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_commented_lambda_single_line_for_generic_task()
        {
            var testExpression = $@"
Func<Task<AsyncMemberMethods>> lambda = async () => ({TestSourceCode.FullTriviaText}await AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}){TestSourceCode.FullTriviaText}.Field1;
";

            var whitespaceCorrectedTrivia = TestSourceCode.TriviaTextCustom(TestSourceCode.DefaultIndents);
            var fixExpression = $@"
Func<Task<AsyncMemberMethods>> lambda = async () => {{
AsyncMemberMethods taskResult =
{whitespaceCorrectedTrivia}await AsyncMethods.GetMemberMethods({TestSourceCode.FullTriviaText}){whitespaceCorrectedTrivia};
return (taskResult){TestSourceCode.FullTriviaText}.Field1;
}};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_single_line_for_generic_task_on_broken_syntax()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () => (await AsyncMethods.GetMemberMethods()).Field1
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = async () => {
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1;
}";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression);
        }

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_lambda_single_line_for_generic_task_with_non_async_broken_syntax()
        {
            var testExpression = @"
Func<Task<AsyncMemberMethods>> lambda = () => (await AsyncMethods.GetMemberMethods()).Field1;
";
            var fixExpression = @"
Func<Task<AsyncMemberMethods>> lambda = () => {
AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
return (taskResult).Field1;
};
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression, true);
        }
        #endregion Lambda Single Line

        /*
        TODO: see how it handles types that are not included in the namespace.
    */
    }
}