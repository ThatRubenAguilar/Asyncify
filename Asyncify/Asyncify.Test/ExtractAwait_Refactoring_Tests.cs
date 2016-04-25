using System.Collections.Generic;
using System.Linq;
using Asyncify.RefactorProviders;
using Asyncify.Test.Helpers.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class ExtractAwait_Refactoring_Tests :
        AwaitTaskRefactoringVerifier<ExtractAwaitExpressionToVariableRefactorProvider>
    {

        protected ResultLocation ExpectedResultLocation(string testExpression, string refactoringTargetCode)
        {
            return ExpectedResultLocation(TaskWrapperCode.MergeCode(testExpression), refactoringTargetCode);
        }

        protected IEnumerable<ResultLocation> ExpectedResultLocations(string testExpression, string blockingCallCode)
        {
            return ExpectedResultLocations(TaskWrapperCode.MergeCode(testExpression), blockingCallCode);
        }

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
        public void Should_extract_correct_await_in_double_await_in_block_code_for_generic_task()
        {
            var testExpression = @"
        {
            Task<Task<AsyncMemberMethods>> doubleAwait = Task.FromResult(Task.FromResult(new AsyncMemberMethods()));
            var t = (await await doubleAwait).Field1;
        }
";
            var fixOuterExpression = @"
        {
            Task<Task<AsyncMemberMethods>> doubleAwait = Task.FromResult(Task.FromResult(new AsyncMemberMethods()));
            AsyncMemberMethods taskResult = await await doubleAwait;
            var t = (taskResult).Field1;
        }
";
            var fixInnerExpression = @"
        {
            Task<Task<AsyncMemberMethods>> doubleAwait = Task.FromResult(Task.FromResult(new AsyncMemberMethods()));
            Task<AsyncMemberMethods> taskResult = await doubleAwait;
            var t = (await taskResult).Field1;
        }
";
            var expected = ExpectedResultLocations(testExpression, "await").ToArray();
            AwaitTaskRefactoring(testExpression, expected[0], fixOuterExpression, true);
            AwaitTaskRefactoring(testExpression, expected[1], fixInnerExpression, true);
        }
        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_block_code_for_generic_task_with_type_in_different_namespace()
        {
            var testExpression = @"
        {
            var t = (await AsyncMethods.GetDifferentMemberMethods()).Field1;
        }
";
            var fixExpression = @"
        {
            Different.AsyncMemberMethods taskResult = await AsyncMethods.GetDifferentMemberMethods();
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
            var formattedTrivia = FullTriviaCode.TriviaTextFormatted(TestSourceCode.DefaultIndents + 1);
            var fixExpression = $@"
        {{
            AsyncMemberMethods taskResult =
{formattedTrivia}await AsyncMethods.GetMemberMethods({formattedTrivia}){formattedTrivia};
            var t = (taskResult){formattedTrivia}.Field1;
        }}
";
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
        public void Should_extract_await_in_lambda_block_code_for_generic_task_with_type_in_different_namespace()
        {
            var testExpression = @"
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {
            return (await AsyncMethods.GetDifferentMemberMethods()).Field1;
        };
";
            var fixExpression = @"
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {
            Different.AsyncMemberMethods taskResult = await AsyncMethods.GetDifferentMemberMethods();
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
            var formattedTrivia = FullTriviaCode.TriviaTextFormatted(TestSourceCode.DefaultIndents+1);
            var fixExpression = $@"
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {{
            AsyncMemberMethods taskResult =
{formattedTrivia}await AsyncMethods.GetMemberMethods({formattedTrivia}){formattedTrivia};
            return (taskResult){formattedTrivia}.Field1;
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
        public void Should_extract_await_in_lambda_single_line_for_generic_task_with_type_in_different_namespace()
        {
            var testExpression = @"
        Func<Task<AsyncMemberMethods>> lambda = async () => (await AsyncMethods.GetDifferentMemberMethods()).Field1;
";
            var fixExpression = @"
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {
            Different.AsyncMemberMethods taskResult = await AsyncMethods.GetDifferentMemberMethods();
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

            var formattedTrivia = FullTriviaCode.TriviaTextFormatted(TestSourceCode.DefaultIndents+1);
            var fixExpression = $@"
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {{
            AsyncMemberMethods taskResult =
{formattedTrivia}await AsyncMethods.GetMemberMethods({formattedTrivia}){formattedTrivia};
            return (taskResult){formattedTrivia}.Field1;
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
        Func<Task<AsyncMemberMethods>> lambda = async () =>
        {
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
        Func<Task<AsyncMemberMethods>> lambda = () =>
        {
            AsyncMemberMethods taskResult = await AsyncMethods.GetMemberMethods();
            return (taskResult).Field1;
        };
";
            var expected = ExpectedResultLocation(testExpression, "await");
            AwaitTaskRefactoring(testExpression, expected, fixExpression, true);
        }
        #endregion Lambda Single Line

        /*
        TODO: see how it handles types that are not included in the namespace, use simplifier/include using, also add 2 deep namespace tests
        TODO: Pipe async definition up 1
        TODO: CS4033 fix with async Task instead of void, split lambda and anonymous method delegates (may need to find type and change base) and method
    */
    }
}