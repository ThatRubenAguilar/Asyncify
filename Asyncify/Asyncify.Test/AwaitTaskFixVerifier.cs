using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        protected static readonly string TaskStaticClass = @"
    using System;
    using System.Threading.Tasks;

    static class AsyncMethods
    {
        public static Task<int> GetNumber()
        {
            return Task.FromResult(42);
        }

        public static Task PerformProcessing()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        public static Task<AsyncMemberMethods> GetMemberMethods()
        {
            return Task.FromResult(new AsyncMemberMethods());
        }
    }
";

        protected static readonly string TaskMemberClass = @"
    using System;
    using System.Threading.Tasks;

    class AsyncMemberMethods
    {
        public AsyncMemberMethods Field1 = null;
        public AsyncMemberMethods Property1 => null;
        public Task Result = null;

        public TaskAwaiter<AsyncMemberMethods> GetAwaiter() 
        {
            return null;
        }
        
        public AsyncMemberMethods GetResult() 
        {
            return null;
        }

        public Task<int> GetNumber()
        {
            return Task.FromResult(42);
        }

        public Task PerformProcessing()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }
        
    }
";


        protected const int TaskExpressionWrapperStartCol = 0;
        protected const int TaskExpressionWrapperStartLine = 9;
        protected static readonly string TaskExpressionWrapper = @"
using System;
using System.Threading.Tasks;

class Test
{{
    async Task TestMethod()
    {{
{0}
    }}
}}
";

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
            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] {testTaskClass, TaskStaticClass, TaskMemberClass}, expected);

            var fixTaskClass = String.Format(TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new[] {TaskStaticClass, TaskMemberClass}, allowNewCompilerDiagnostics:allowNewCompilerDiagnostics);
        }

        protected Tuple<int, int> FindLineAndColOffset(string source, string expectedSyntax)
        {
            var lineColOffsetsList = FindLineAndColOffsets(source, expectedSyntax);
            if (lineColOffsetsList.Count > 1)
                throw new ArgumentOutOfRangeException(nameof(expectedSyntax),
                    $"Expected to find 0 or 1 matches, found {lineColOffsetsList.Count}");

            return lineColOffsetsList.FirstOrDefault();
        }

        protected IList<Tuple<int, int>> FindLineAndColOffsets(string source, string expectedSyntax)
        {
            var syntaxRegex = new Regex(expectedSyntax);
            var matches = syntaxRegex.Matches(source);
            var lineColTupleList = new List<Tuple<int, int>>();
            var newLineEndingIndices = FindNewLineEndingLocations(source);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var lineOffset = 0;
                    var colOffset = match.Index + 1;
                    foreach (var newLineEndingIndex in newLineEndingIndices)
                    {
                        if (newLineEndingIndex > match.Index)
                            break;
                        colOffset = match.Index - newLineEndingIndex;
                        lineOffset++;
                    }
                    lineColTupleList.Add(new Tuple<int,int>(lineOffset, colOffset));
                }
            }

            return lineColTupleList;
        }

        /// <summary>
        /// Finds all the ending indices of a newline within a string
        /// </summary>
        /// <param name="input">string to find newlines in</param>
        /// <returns>A list of the last indices of a newline sequences within the string</returns>
        protected static IList<int> FindNewLineEndingLocations(string input)
        {
            var newLineCharArray = Environment.NewLine.ToCharArray();
            var newLineLocationList = new List<int>();
            if (newLineCharArray.Length > 1)
            {
                int matchIndex = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == newLineCharArray[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex >= newLineCharArray.Length)
                        {
                            matchIndex = 0;
                            newLineLocationList.Add(i);
                        }
                    }
                    else
                    {
                        matchIndex = 0;
                    }
                }
            }
            else
            {
                var singleNewLineChar = newLineCharArray[0];
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == singleNewLineChar)
                        newLineLocationList.Add(i);
                }
            }
            return newLineLocationList;
        }
    }
}