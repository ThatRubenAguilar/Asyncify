using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Asyncify.Test.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelper
{
    public abstract class RefactoringFixVerifier
    {
        protected virtual CodeRefactoringProvider GetCSharpRefactoringProvider()
        {
            return null;
        }


        /// <summary>
        /// Called to test a C# refactoring when applied on the inputted string as a source
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="supportingSources">Classes in the form of strings which support the oldSource</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        protected void VerifyCSharpRefactoring(string oldSource, ResultLocation expected, string newSource, string[] supportingSources = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyRefactoring(LanguageNames.CSharp, GetCSharpRefactoringProvider(), oldSource, expected, newSource, supportingSources, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// General verifier for refactorings.
        /// Creates a Document from the source string, then applies relevant refactorings.
        /// Then gets the string after the refactoring is applied and compares it with the expected result.
        /// Note: If any refactoring causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="language">The language the source code is in</param>
        /// <param name="refactoringProvider">Refactoring to apply to sources</param>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="expected">Result expected to be fixed and found</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="supportingSources">Classes in the form of strings which support the oldSource</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        private void VerifyRefactoring(string language, CodeRefactoringProvider refactoringProvider, string oldSource, ResultLocation expected, string newSource, string[] supportingSources, bool allowNewCompilerDiagnostics)
        {
            var allOldSources = new List<string>();
            allOldSources.Add(oldSource);
            if (supportingSources != null)
                allOldSources.AddRange(supportingSources);

            var document = DiagnosticVerifier.CreateDocuments(allOldSources.ToArray(), language).First();
            var compilerDiagnostics = CodeFixVerifier.GetCompilerDiagnostics(document);
            
            var actions = new List<CodeAction>();

            TextSpan? fullSpan = expected?.Span;
            if (expected == null)
                fullSpan = document.GetSyntaxRootAsync().Result.FullSpan;

            var context = new CodeRefactoringContext(document, fullSpan.Value, (a) => actions.Add(a), CancellationToken.None);

            refactoringProvider.ComputeRefactoringsAsync(context).Wait();


            if (actions.Any())
            {
                document = CodeFixVerifier.ApplyFix(document, actions.ElementAt(0));
            }

            var newCompilerDiagnostics = CodeFixVerifier.GetNewDiagnostics(compilerDiagnostics, CodeFixVerifier.GetCompilerDiagnostics(document));

            //check if applying the code fix introduced any new compiler diagnostics
            if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
            {
                // Format and get the compiler diagnostics again so that the locations make sense in the output
                document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                newCompilerDiagnostics = CodeFixVerifier.GetNewDiagnostics(compilerDiagnostics, CodeFixVerifier.GetCompilerDiagnostics(document));

                Assert.IsTrue(false,
                    string.Format("Refactoring introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                        string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                        document.GetSyntaxRootAsync().Result.ToFullString()));
            }
            
            

            //after applying all of the code fixes, compare the resulting string to the inputted one
            var actual = CodeFixVerifier.GetStringFromDocument(document);

            var mismatchIndex = CodeFixVerifier.DiffersAtIndex(newSource, actual);
            if (mismatchIndex != -1)
            {
                var lineColOffset = newSource.FindSourceLocation(mismatchIndex);
                Assert.AreEqual(newSource, actual, $"Sources differ at line {lineColOffset.Line} and column {lineColOffset.Column}. Expected '{newSource.GetLine(lineColOffset.Line)}', but Actual '{actual.GetLine(lineColOffset.Line)}'");
            }
            else
                Assert.AreEqual(newSource, actual); // Fallback in case DifferAtIndex fails.
        }
    }
}