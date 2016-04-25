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
                new TaskStaticCode(), new TaskMemberCode(), new TaskChildCode(), new TaskNamespacedStaticCode(),
            });

        protected static readonly TaskExpressionWrapper TaskWrapperCode = new TaskExpressionWrapper();

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TAnalyzer();
        }
        

        protected void AwaitTaskDiagnosticAndFix(MergedCodeUnit testExpression, DiagnosticResult expected,
            MergedCodeUnit fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = testExpression;
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);

            var fixTaskClass = fixedExpression;
            VerifyCSharpFix(testTaskClass.ToString(), fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics, expected);
        }
        protected void AwaitTaskDiagnosticsAndFix(MergedCodeUnit testExpression, DiagnosticResult[] expected,
            MergedCodeUnit fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = testExpression;
            VerifyCSharpDiagnostic(TaskWrapperProject.TestCodeCompilationUnit(testTaskClass), expected);

            var fixTaskClass = fixedExpression;
            VerifyCSharpFix(testTaskClass.ToString(), fixTaskClass.ToString(), TaskWrapperProject.SupportingSourcesAsString(), allowNewCompilerDiagnostics);
        }


        protected DiagnosticResult AwaitTaskExpectedResult(MergedCodeUnit testExpression,
            DiagnosticDescriptor rule, string blockingCallCode, string callerTaskExpression)
        {
            return AwaitTaskExpectedResults(testExpression, rule, blockingCallCode, callerTaskExpression).Single();
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