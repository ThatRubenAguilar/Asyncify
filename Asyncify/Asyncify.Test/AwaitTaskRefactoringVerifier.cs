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
                new TaskStaticCode(), new TaskMemberCode(), new TaskChildCode(), new TaskNamespacedStaticCode(), new TaskNamespacedMemberCode(),
            });
        

        protected override CodeRefactoringProvider GetCSharpRefactoringProvider()
        {
            return new TRefactoring();
        }

        // TODO: Refactor fix verifier to this pattern of generic WrapperCodeUnit new() -> merged code unit function in all levels of test framework
        protected void AwaitTaskRefactoring<TWrapper>(string testExpression, ResultLocation expected, 
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            var testTaskClass = wrapper.MergeCode( testExpression);

            var fixTaskClass = wrapper.MergeCode( fixedExpression);
            VerifyCSharpRefactoring(testTaskClass.ToString(), expected, fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics);
        }
        protected void AwaitTaskRefactoring<TWrapper>(string[] testExpressions, ResultLocation expected, 
            string[] fixedExpressions, bool allowNewCompilerDiagnostics = false)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            var testTaskClass = wrapper.MergeCode( testExpressions);

            var fixTaskClass = wrapper.MergeCode( fixedExpressions);
            VerifyCSharpRefactoring(testTaskClass.ToString(), expected, fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics);
        }
        
        protected ResultLocation ExpectedResultLocation<TWrapper>(string refactoringTargetCode, params string[] testExpressions)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            return ExpectedResultLocation(refactoringTargetCode, wrapper.MergeCode(testExpressions));
        }

        protected IEnumerable<ResultLocation> ExpectedResultLocations<TWrapper>(string blockingCallCode, params string[] testExpressions)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            return ExpectedResultLocations(blockingCallCode, wrapper.MergeCode(testExpressions));
        }

        protected ResultLocation ExpectedResultLocation(string refactoringTargetCode, MergedCodeUnit testExpression)
        {
            return ExpectedResultLocations(refactoringTargetCode, testExpression).Single();
        }

        protected IEnumerable<ResultLocation> ExpectedResultLocations(string blockingCallCode, MergedCodeUnit testExpression)
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