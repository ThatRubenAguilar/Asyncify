using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TestHelper;

namespace Asyncify.Test.Extensions
{
    public static class SourceCodeTextExtensions
    {
        private static readonly char[] NewLineCharArray = Environment.NewLine.ToCharArray();

        /// <summary>
        /// Finds one source location of the expectedSyntax. Errors on more than 1 found.
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="expectedSyntax">expected syntax to find</param>
        /// <returns>source location of the expected syntax</returns>
        public static ResultLocation FindSourceLocation(this string source, string expectedSyntax)
        {
            var lineColOffsetsList = FindSourceLocations(source, expectedSyntax);
            if (lineColOffsetsList.Count > 1)
                throw new ArgumentOutOfRangeException(nameof(expectedSyntax),
                    $"Expected to find 0 or 1 matches, found {lineColOffsetsList.Count}");

            return lineColOffsetsList.FirstOrDefault();
        }

        /// <summary>
        /// Finds all source locations of the expectedSyntax
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="expectedSyntax">expected syntax to find</param>
        /// <returns>source locations of the expected syntax</returns>
        public static IList<ResultLocation> FindSourceLocations(this string source, string expectedSyntax)
        {
            var syntaxRegex = new Regex(Regex.Escape(expectedSyntax));
            var matches = syntaxRegex.Matches(source);
            var lineColTupleList = new List<ResultLocation>();
            var newLineEndingIndices = FindNewLineEndingLocations(source);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var lineOffset = 1;
                    var colOffset = match.Index + 1;
                    foreach (var newLineEndingIndex in newLineEndingIndices)
                    {
                        if (newLineEndingIndex > match.Index)
                            break;
                        colOffset = match.Index - newLineEndingIndex;
                        lineOffset++;
                    }
                    lineColTupleList.Add(new ResultLocation(lineOffset, colOffset, match.Index, expectedSyntax.Length));
                }
            }

            return lineColTupleList;
        }

        /// <summary>
        /// Finds the source location of a sourceIndex
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="sourceIndex">index within the source</param>
        /// <returns>source location of the source index</returns>
        public static ResultLocation FindSourceLocation(this string source, int sourceIndex)
        {
            if (sourceIndex >= source.Length)
                throw new ArgumentException($"{nameof(sourceIndex)} ({sourceIndex}) cannot be greater or equal to {nameof(source)}.Length ({source.Length})");

            var newLineEndingIndices = FindNewLineEndingLocations(source, sourceIndex);
            if (newLineEndingIndices.Count == 0)
                return new ResultLocation(1, sourceIndex+1, sourceIndex, 0);

            var lineOffset = newLineEndingIndices.Count;
            var closestNewLineIndex = newLineEndingIndices.Last();
            var colOffset = sourceIndex - closestNewLineIndex;
            return new ResultLocation(lineOffset+1, colOffset, sourceIndex, 0);
        }

        /// <summary>
        /// Gets the text of the line
        /// </summary>
        public static string GetLine(this string source, int line)
        {
            var zeroAlignedLine = line - 1;
            var allLineEndings = source.FindNewLineEndingLocations();
            var allLineEndEnum = allLineEndings.GetEnumerator();
            int count = 0;
            while (count < zeroAlignedLine && allLineEndEnum.MoveNext())
            {
                count++;
            }
            if (count < zeroAlignedLine)
                throw new ArgumentOutOfRangeException(nameof(line), $"{nameof(source)} has {allLineEndings.Count} lines, attempted to access {line} line.");

            int startIndex;
            if (count != 0)
                startIndex = allLineEndEnum.Current + 1; // Newline ending will include 1 char of the newline char array
            else
                startIndex = 0;
            int endIndex;
            if (allLineEndEnum.MoveNext())
                endIndex = allLineEndEnum.Current - (NewLineCharArray.Length - 1); // Newline ending will include the n-1 rest of the newline char array
            else
                endIndex = source.Length;
            
            return source.Substring(startIndex, (endIndex - startIndex));
        }

        /// <summary>
        /// Finds all the ending indices of a newline within a string
        /// </summary>
        /// <param name="input">string to find newlines in</param>
        /// <param name="stopIndex">an optional index to stop checking early</param>
        /// <returns>A list of the last indices of a newline sequences within the string</returns>
        public static IList<int> FindNewLineEndingLocations(this string input, int stopIndex = -1)
        {
            if (stopIndex < 0)
                return FindAllNewLineEndlingLocations(input);
            return FindNewLineEndlingLocationsUntilIndex(input, stopIndex);
        }

        private static IList<int> FindNewLineEndlingLocationsUntilIndex(string input, int stopIndex)
        {
            var newLineLocationList = new List<int>();
            if (NewLineCharArray.Length > 1)
            {
                int matchIndex = 0;
                for (int i = 0; i < input.Length && i <= stopIndex; i++)
                {
                    if (input[i] == NewLineCharArray[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex >= NewLineCharArray.Length)
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
                var singleNewLineChar = NewLineCharArray[0];
                for (int i = 0; i < input.Length && i <= stopIndex; i++)
                {
                    if (input[i] == singleNewLineChar)
                        newLineLocationList.Add(i);
                }
            }
            return newLineLocationList;
        }
        private static IList<int> FindAllNewLineEndlingLocations(string input)
        {
            var newLineLocationList = new List<int>();
            if (NewLineCharArray.Length > 1)
            {
                int matchIndex = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == NewLineCharArray[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex >= NewLineCharArray.Length)
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
                var singleNewLineChar = NewLineCharArray[0];
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