using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Test.Extensions;
using Asyncify.Test.Helpers.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using TestHelper;

namespace Asyncify.Test
{
    public class AwaitTaskRefactoringVerifier<TRefactoring> : RefactoringFixVerifier
        where TRefactoring : CodeRefactoringProvider, new()
    {

        protected static readonly ProjectUnit TaskWrapperProject = new ProjectUnit(
            new SourceCodeUnit[]
            {
                new TaskStaticCode(), new TaskMemberCode(), new TaskChildCode(), new TaskNamespacedStaticCode(),
            });

        protected static readonly TaskExpressionWrapper TaskWrapperCode = new TaskExpressionWrapper();

        protected override CodeRefactoringProvider GetCSharpRefactoringProvider()
        {
            return new TRefactoring();
        }


        protected void AwaitTaskRefactoring(string testExpression, ResultLocation expected, 
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = TaskWrapperCode.MergeCode( testExpression);

            var fixTaskClass = TaskWrapperCode.MergeCode( fixedExpression);
            VerifyCSharpRefactoring(testTaskClass.ToString(), expected, fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics);
        }

        protected ResultLocation ExpectedResultLocation(MergedCodeUnit testExpression, string refactoringTargetCode)
        {
            return ExpectedResultLocations(testExpression, refactoringTargetCode).Single();
        }

        protected IEnumerable<ResultLocation> ExpectedResultLocations(MergedCodeUnit testExpression, string blockingCallCode)
        {
            var lineColOffsets = (IEnumerable<ResultLocation>)testExpression.FindAbsoluteSourceLocations(0, blockingCallCode);
            foreach (var lineColOffset in lineColOffsets)
            {
                yield return lineColOffset;
            }
            yield break;
        }
    }
}