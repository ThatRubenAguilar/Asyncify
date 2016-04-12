using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Test.Extensions;
using Microsoft.CodeAnalysis;
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


        protected void AwaitTaskRefactoring(string testExpression, ResultLocation expected, 
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);

            var fixTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, fixedExpression);
            VerifyCSharpRefactoring(testTaskClass, expected, fixTaskClass, new[] { TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, allowNewCompilerDiagnostics);
        }

        protected ResultLocation ExpectedResultLocation(string testExpression, string refactoringTargetCode)
        {
            return ExpectedResultLocations(testExpression, refactoringTargetCode).Single();
        }

        protected IEnumerable<ResultLocation> ExpectedResultLocations(string testExpression, string blockingCallCode)
        {
            var lineColOffsets = testExpression.FindSourceLocations(blockingCallCode);
            foreach (var lineColOffset in lineColOffsets)
            {
                var absoluteLocation = TestSourceCode.TaskExpressionWrapperLocation.Add(lineColOffset);
                        
                yield return absoluteLocation;
            }
            yield break;
        }
    }
}