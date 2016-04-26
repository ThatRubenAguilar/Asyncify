using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Asyncify.Test.Extensions;
using Asyncify.Test.Helpers.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace Asyncify.Test
{
    public class AwaitTaskFixVerifier<TAnalyzer, TProvider> : CodeFixVerifier
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TProvider : CodeFixProvider, new()
    {

        protected static readonly ProjectUnit TaskWrapperProject = new ProjectUnit(
            new SourceCodeUnit[]
            {
                new TaskStaticCode(), new TaskMemberCode(), new TaskChildCode(), new TaskNamespacedStaticCode(), new TaskNamespacedMemberCode(), 
            });
        

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TAnalyzer();
        }

        protected void VerifyNoDiagnostic<TWrapper>(params string[] testExpressions)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            var testTaskClass = wrapper.MergeCode(testExpressions);
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass));
        }

        protected void AwaitTaskDiagnosticAndFix<TWrapper>(string testExpression, DiagnosticResult expected,
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            AwaitTaskDiagnosticAndFix(wrapper.MergeCode(testExpression), expected, wrapper.MergeCode(fixedExpression), allowNewCompilerDiagnostics);
        }

        protected void AwaitTaskDiagnosticAndFix(MergedCodeUnit testExpression, DiagnosticResult expected,
            MergedCodeUnit fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = testExpression;
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);

            var fixTaskClass = fixedExpression;
            VerifyCSharpFix(testTaskClass.ToString(), fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics, expected);
        }

        protected void AwaitTaskDiagnosticsAndFix<TWrapper>(string testExpression, DiagnosticResult[] expected,
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            AwaitTaskDiagnosticsAndFix(wrapper.MergeCode(testExpression), expected, wrapper.MergeCode(fixedExpression), allowNewCompilerDiagnostics);
        }

        protected void AwaitTaskDiagnosticsAndFix(MergedCodeUnit testExpression, DiagnosticResult[] expected,
            MergedCodeUnit fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = testExpression;
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);

            var fixTaskClass = fixedExpression;
            VerifyCSharpFix(testTaskClass.ToString(), fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics);
        }


        protected DiagnosticResult AwaitTaskExpectedResult<TWrapper>(string testExpression,
            DiagnosticDescriptor rule, string blockingCallCode, string callerTaskExpression)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            return AwaitTaskExpectedResult(wrapper.MergeCode(testExpression), rule, blockingCallCode, callerTaskExpression);
        }
        protected DiagnosticResult AwaitTaskExpectedResult(MergedCodeUnit testExpression,
            DiagnosticDescriptor rule, string blockingCallCode, string callerTaskExpression)
        {
            return AwaitTaskExpectedResults(testExpression, rule, blockingCallCode, callerTaskExpression).Single();
        }

        protected IEnumerable<DiagnosticResult> AwaitTaskExpectedResults<TWrapper>(string testExpression,
            DiagnosticDescriptor rule, string blockingCallCode, params string[] callerTaskExpressions)
            where TWrapper : WrapperCodeUnit, new()
        {
            var wrapper = new TWrapper();
            return AwaitTaskExpectedResults(wrapper.MergeCode(testExpression), rule, blockingCallCode, callerTaskExpressions);
        }

        protected IEnumerable<DiagnosticResult> AwaitTaskExpectedResults(MergedCodeUnit testExpression, DiagnosticDescriptor rule, string blockingCallCode, params string[] callerTaskExpressions)
        {
            var lineColOffsets = (IEnumerable<ResultLocation>)testExpression.FindAbsoluteSourceLocations(0,blockingCallCode);
            var lineColOffsetsEnum = lineColOffsets.GetEnumerator();
            var callerTaskExprEnum = callerTaskExpressions.GetEnumerator();
            while(lineColOffsetsEnum.MoveNext() && callerTaskExprEnum.MoveNext())
            {
                var lineColOffset = lineColOffsetsEnum.Current;
                var callerTaskExpression = callerTaskExprEnum.Current;

                var expected = new DiagnosticResult
                {
                    Id = rule.Id,
                    Message = String.Format(rule.MessageFormat.ToString(),
                        callerTaskExpression),
                    Severity = rule.DefaultSeverity,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", lineColOffset)
                        }
                };
                yield return expected;
            }
            yield break;
        }

    }
}