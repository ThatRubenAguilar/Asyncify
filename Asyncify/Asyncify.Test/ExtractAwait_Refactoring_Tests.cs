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

        [TestMethod, TestCategory("Extract_Await")]
        public void Should_extract_await_in_block_code()
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

        /*
        TODO: 
        see how it handles types that are not included in the namespace.
        lambda block
        lambda single line
        comment code block
        comment lambda block
        comment lambda single line
        await not used
        await not generic
        code block syntax error
        lambda block syntax error
        lambda single line syntax error
    */
    }
}