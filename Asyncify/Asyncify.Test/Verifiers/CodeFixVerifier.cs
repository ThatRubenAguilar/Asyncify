using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Asyncify.Test.Extensions;

namespace TestHelper
{
    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with codefixes.
    /// Contains methods used to verify correctness of codefixes
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Returns the codefix being tested (C#) - to be implemented in non-abstract class
        /// </summary>
        /// <returns>The CodeFixProvider to be used for CSharp code</returns>
        protected virtual CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        /// <summary>
        /// Returns the codefix being tested (VB) - to be implemented in non-abstract class
        /// </summary>
        /// <returns>The CodeFixProvider to be used for VisualBasic code</returns>
        protected virtual CodeFixProvider GetBasicCodeFixProvider()
        {
            return null;
        }

        /// <summary>
        /// Called to test a C# codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="supportingSources">Classes in the form of strings which support the oldSource</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        /// <param name="fixSpecificResult">Specific result to fix, leaving the rest unfixed.</param>
        protected void VerifyCSharpFix(string oldSource, string newSource, string[] supportingSources = null, bool allowNewCompilerDiagnostics = false, DiagnosticResult? fixSpecificResult = null)
        {
            VerifyFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, supportingSources, allowNewCompilerDiagnostics, fixSpecificResult);
        }

        /// <summary>
        /// Called to test a VB codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="supportingSources">Classes in the form of strings which support the oldSource</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        /// <param name="fixSpecificResult">Specific result to fix, leaving the rest unfixed.</param>
        protected void VerifyBasicFix(string oldSource, string newSource, string[] supportingSources = null, bool allowNewCompilerDiagnostics = false, DiagnosticResult? fixSpecificResult = null)
        {
            VerifyFix(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, supportingSources, allowNewCompilerDiagnostics, fixSpecificResult);
        }

        /// <summary>
        /// General verifier for codefixes.
        /// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
        /// Then gets the string after the codefix is applied and compares it with the expected result.
        /// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="language">The language the source code is in</param>
        /// <param name="analyzer">The analyzer to be applied to the source code</param>
        /// <param name="codeFixProvider">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="supportingSources">Classes in the form of strings which support the oldSource</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        /// <param name="fixSpecificResult">Specific result to fix, leaving the rest unfixed.</param>
        private void VerifyFix(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, string[] supportingSources, bool allowNewCompilerDiagnostics, DiagnosticResult? fixSpecificResult = null)
        {
            var allOldSources = new List<string>();
            allOldSources.Add(oldSource);
            if (supportingSources != null)
                allOldSources.AddRange(supportingSources);

            var document = CreateDocuments(allOldSources.ToArray(), language).First();
            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
            var compilerDiagnostics = GetCompilerDiagnostics(document);
            var attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                Diagnostic applyDiagnostic = null;
                foreach (var analyzerDiagnostic in analyzerDiagnostics)
                {
                    if (fixSpecificResult == null || fixSpecificResult.Value.Equals(analyzerDiagnostic))
                    {
                        applyDiagnostic = analyzerDiagnostic;
                        break;
                    }
                }

                if (applyDiagnostic == null)
                    break;

                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, applyDiagnostic, (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }
                
                document = ApplyFix(document, actions.ElementAt(0));
                analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                //check if applying the code fix introduced any new compiler diagnostics
                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                    Assert.IsTrue(false,
                        string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                            string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                            document.GetSyntaxRootAsync().Result.ToFullString()));
                }

                //check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            //after applying all of the code fixes, compare the resulting string to the inputted one
            var actual = GetStringFromDocument(document);

            var mismatchIndex = DiffersAtIndex(newSource, actual);
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