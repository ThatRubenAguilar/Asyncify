using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TestHelper
{
    public class ResultLocation : IEquatable<ResultLocation>, IEquatable<Location>
    {

        public ResultLocation(int line, int column, int start, int length) :
            this(line, column, new TextSpan(start, length))
        {
        }

        public ResultLocation(int line, int column, TextSpan span)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            this.Span = span;
            this.Line = line;
            this.Column = column;
        }

        public TextSpan Span { get; }
        
        public int Line { get; }
        public int Column { get; }

        public ResultLocation Add(ResultLocation b)
        {
            return new ResultLocation(Line + b.Line - 1, Column + b.Column - 1, Span.Start + b.Span.Start, Span.Length + b.Span.Length);
        }
        public ResultLocation AddLines(int lines)
        {
            return new ResultLocation(Line + lines, Column, Span.Start, Span.Length);
        }

        public bool Equals(ResultLocation other)
        {
            return Span.Equals(other.Span) && Line == other.Line && Column == other.Column;
        }

        public bool Equals(Location other)
        {
            if (other == null)
                return false;

            var actualSpan = other.GetLineSpan();

            var actualLinePosition = actualSpan.StartLinePosition;
            
            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0 && actualLinePosition.Line + 1 != Line)
                return false;

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0 && actualLinePosition.Character + 1 != Column)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ResultLocation && Equals((ResultLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Span.GetHashCode();
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Column;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Line: {Line}, Column: {Column}, Span(Start, Length, End): {{{Span.Start}, {Span.Length}, {Span.End} }}";
        }
    }
}