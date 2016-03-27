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

    }
}