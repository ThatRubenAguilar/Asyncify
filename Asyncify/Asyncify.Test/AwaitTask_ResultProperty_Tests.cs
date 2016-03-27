using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class AwaitTask_ResultProperty_Tests : AwaitTaskFixVerifier<ConsiderAwaitOverResultAnalyzer, ConsiderAwaitOverResultCodeFixProvider>
    {
        [TestMethod]
        public void Should_have_no_diagnostic_for_empty_code()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod]
        public void Should_await_task_fix_on_generic_task_result_property()
        {
            var testExpression = @"var val = AsyncMethods.GetNumber().Result;";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(), 
                                            "AsyncMethods.GetNumber()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 44)
                        }
            };
            var fixExpression = @"var val = await AsyncMethods.GetNumber();";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Field1;";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 51)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_field()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Field1;";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 52)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Field1;";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.Property1;";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 51)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_property()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).Property1;";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 52)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).Property1;";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_add_parenthesis_to_await_task_fix_on_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = AsyncMethods.GetMemberMethods().Result.GetNumber();";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 51)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

        [TestMethod]
        public void Should_not_add_parenthesis_to_await_task_fix_on_already_parenthesized_generic_task_result_property_when_using_task_method()
        {
            var testExpression = @"var val = (AsyncMethods.GetMemberMethods().Result).GetNumber();";
            var expected = new DiagnosticResult
            {
                Id = ConsiderAwaitOverResultAnalyzer.DiagnosticId,
                Message = String.Format(ConsiderAwaitOverResultAnalyzer.MessageFormat.ToString(),
                                            "AsyncMethods.GetMemberMethods()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 52)
                        }
            };
            var fixExpression = @"var val = (await AsyncMethods.GetMemberMethods()).GetNumber();";

            AwaitTaskDiagnosticAndFix(testExpression, expected, fixExpression);
        }

    }
}