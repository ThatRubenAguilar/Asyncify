using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Asyncify.Test.Extensions;
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
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TAnalyzer();
        }
        

        protected void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected,
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] {testTaskClass, TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, expected);

            var fixTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new[] {TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, allowNewCompilerDiagnostics, expected);
        }
        protected void AwaitTaskDiagnosticsAndFix(string testExpression, DiagnosticResult[] expected,
            string fixedExpression, bool allowNewCompilerDiagnostics = false)
        {
            var testTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] {testTaskClass, TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, expected);

            var fixTaskClass = String.Format(TestSourceCode.TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new[] {TestSourceCode.TaskStaticClass, TestSourceCode.TaskMemberClass, TestSourceCode.TaskChildClass }, allowNewCompilerDiagnostics);
        }


        protected DiagnosticResult AwaitTaskExpectedResult(string testExpression,
            DiagnosticDescriptor rule, string blockingCallCode, string callerTaskExpression)
        {
            return AwaitTaskExpectedResults(testExpression, rule, blockingCallCode, callerTaskExpression).Single();
        }

        protected IEnumerable<DiagnosticResult> AwaitTaskExpectedResults(string testExpression, DiagnosticDescriptor rule, string blockingCallCode, params string[] callerTaskExpressions)
        {
            var lineColOffsets = testExpression.FindSourceLocations(blockingCallCode);
            var lineColOffsetsEnum = lineColOffsets.GetEnumerator();
            var callerTaskExprEnum = callerTaskExpressions.GetEnumerator();
            while(lineColOffsetsEnum.MoveNext() && callerTaskExprEnum.MoveNext())
            {
                var lineColOffset = lineColOffsetsEnum.Current;
                var callerTaskExpression = callerTaskExprEnum.Current;

                var absoluteLocation = TestSourceCode.TaskExpressionWrapperLocation.Add(lineColOffset);
                var expected = new DiagnosticResult
                {
                    Id = rule.Id,
                    Message = String.Format(rule.MessageFormat.ToString(),
                        callerTaskExpression),
                    Severity = rule.DefaultSeverity,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", absoluteLocation)
                        }
                };
                yield return expected;
            }
            yield break;
        }

    }
}