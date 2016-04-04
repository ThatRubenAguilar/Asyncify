using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Asyncify.Test.Extensions
{
    public static class SourceCodeTextExtensions
    {
        /// <summary>
        /// Finds one line and column offset of the expectedSyntax. Errors on more than 1 found.
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="expectedSyntax">expected syntax to find</param>
        /// <returns>tuple of line and column offsets of the start location of the expected syntax</returns>
        public static Tuple<int, int> FindLineAndColOffset(this string source, string expectedSyntax)
        {
            var lineColOffsetsList = FindLineAndColOffsets(source, expectedSyntax);
            if (lineColOffsetsList.Count > 1)
                throw new ArgumentOutOfRangeException(nameof(expectedSyntax),
                    $"Expected to find 0 or 1 matches, found {lineColOffsetsList.Count}");

            return lineColOffsetsList.FirstOrDefault();
        }

        /// <summary>
        /// Finds all line and column offsets of the expectedSyntax
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="expectedSyntax">expected syntax to find</param>
        /// <returns>tuple of line and column offsets of the start locations of the expected syntax</returns>
        public static IList<Tuple<int, int>> FindLineAndColOffsets(this string source, string expectedSyntax)
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
                    lineColTupleList.Add(new Tuple<int, int>(lineOffset, colOffset));
                }
            }

            return lineColTupleList;
        }

        /// <summary>
        /// Finds the line and column offset of a sourceIndex
        /// </summary>
        /// <param name="source">source text to search</param>
        /// <param name="sourceIndex">index within the source</param>
        /// <returns>tuple of line and column offset of the source index</returns>
        public static Tuple<int, int> FindLineAndColOffset(this string source, int sourceIndex)
        {
            if (sourceIndex >= source.Length)
                throw new ArgumentException($"{nameof(sourceIndex)} ({sourceIndex}) cannot be greater or equal to {nameof(source)}.Length ({source.Length})");

            var newLineEndingIndices = FindNewLineEndingLocations(source, sourceIndex);
            if (newLineEndingIndices.Count == 0)
                return new Tuple<int, int>(0, sourceIndex);

            var lineOffset = newLineEndingIndices.Count;
            var closestNewLineIndex = newLineEndingIndices.Last();
            var colOffset = sourceIndex - closestNewLineIndex;
            return new Tuple<int, int>(lineOffset, colOffset);
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
                return FineAllNewLineEndlingLocations(input);
            return FineNewLineEndlingLocationsUntilIndex(input, stopIndex);
        }

        private static IList<int> FineNewLineEndlingLocationsUntilIndex(string input, int stopIndex)
        {
            var newLineCharArray = Environment.NewLine.ToCharArray();
            var newLineLocationList = new List<int>();
            if (newLineCharArray.Length > 1)
            {
                int matchIndex = 0;
                for (int i = 0; i < input.Length && i <= stopIndex; i++)
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
                for (int i = 0; i < input.Length && i <= stopIndex; i++)
                {
                    if (input[i] == singleNewLineChar)
                        newLineLocationList.Add(i);
                }
            }
            return newLineLocationList;
        }
        private static IList<int> FineAllNewLineEndlingLocations(string input)
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