using System;
using Microsoft.CodeAnalysis.CodeRefactorings;
using TestHelper;

namespace Asyncify.Test
{
    public class AwaitTaskRefactoringVerifier<TRefactoring> : RefactoringFixVerifier
        where TRefactoring : CodeRefactoringProvider, new()
    {
        protected override CodeRefactoringProvider GetCSharpRefactoringProvider()
        {
            return new TRefactoring();
        }


        protected void AwaitTaskRefactoring(string testExpression, 
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);

            var fixTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, fixedExpression);
            VerifyCSharpRefactoring(testTaskClass, fixTaskClass, new[] { TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, allowNewCompilerDiagnostics);
        }
    }
}