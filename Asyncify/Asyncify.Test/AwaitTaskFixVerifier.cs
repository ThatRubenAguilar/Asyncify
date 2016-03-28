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
        // Starts at line 9 and col 0
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
        protected const int TaskExpressionWrapperStartCol = 9;
        protected const int TaskExpressionWrapperStartLine = 9;

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TAnalyzer();
        }

        protected void AwaitTaskDiagnosticAndFix(string testExpression, DiagnosticResult expected,
            string fixedExpression)
        {
            var testTaskClass = String.Format(TaskExpressionWrapper, testExpression);
            VerifyCSharpDiagnostic(new[] {testTaskClass, TaskStaticClass, TaskMemberClass}, expected);

            var fixTaskClass = String.Format(TaskExpressionWrapper, fixedExpression);
            VerifyCSharpFix(testTaskClass, fixTaskClass, new[] {TaskStaticClass, TaskMemberClass});
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
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var matchPrefix = source.Substring(0, source.Length - (source.Length - match.Index));
                    var lineOffset = CountNewLine(matchPrefix);
                    var colOffset = match.Index;
                    lineColTupleList.Add(new Tuple<int,int>(lineOffset, colOffset));
                }
            }

            return lineColTupleList;
        }

        protected static int CountNewLine(string input)
        {
            var newLineCharArray = Environment.NewLine.ToCharArray();
            int newLines = 0;
            if (newLineCharArray.Length > 1)
            {
                int matchIndex = 0;
                foreach (var ch in input)
                {
                    if (ch == newLineCharArray[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex >= newLineCharArray.Length)
                        {
                            matchIndex = 0;
                            newLines++;
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
                foreach (var ch in input)
                {
                    if (ch == singleNewLineChar)
                        newLines++;
                }
            }
            return newLines;
        }
    }
}