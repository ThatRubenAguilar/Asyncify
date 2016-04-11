using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace TestHelper
{
    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    public struct DiagnosticResultLocation : IEquatable<DiagnosticResultLocation>, IEquatable<Location>
    {
        

        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            this.Path = path;
            this.Line = line;
            this.Column = column;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }

        public bool Equals(DiagnosticResultLocation other)
        {
            return string.Equals(Path, other.Path) && Line == other.Line && Column == other.Column;
        }

        public bool Equals(Location other)
        {
            if (other == null)
                return false;

            var actualSpan = other.GetLineSpan();

            if (actualSpan.Path != Path)
                return false;

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
            return obj is DiagnosticResultLocation && Equals((DiagnosticResultLocation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Line;
                hashCode = (hashCode*397) ^ Column;
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public struct DiagnosticResult : IEquatable<Diagnostic>, IEquatable<DiagnosticResult>
    {

        private DiagnosticResultLocation[] locations;

        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (this.locations == null)
                {
                    this.locations = new DiagnosticResultLocation[] { };
                }
                return this.locations;
            }

            set
            {
                this.locations = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Path : "";
            }
        }

        public int Line
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Line : -1;
            }
        }

        public int Column
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Column : -1;
            }
        }

        public bool Equals(Diagnostic other)
        {
            return Equals(other, true);
        }

        public bool Equals(Diagnostic other, bool includeLocation)
        {
            if (other == null)
                return false;

            if (Severity != other.Severity || !string.Equals(Id, other.Id) ||
                !string.Equals(Message, other.GetMessage()))
                return false;

            if (!includeLocation)
                return true;

            if (Line == -1 && Column == -1)
            {
                if (other.Location != Location.None)
                    return false;
            }
            else
            {
                var firstLocation = Locations.First();
                if (!firstLocation.Equals(other.Location))
                    return false;

                var additionalLocations = other.AdditionalLocations.ToArray();
                
                if (additionalLocations.Length != Locations.Length - 1)
                    return false;

                for (int j = 0; j < additionalLocations.Length; ++j)
                {
                    if (!Locations[j + 1].Equals(additionalLocations[j]))
                        return false;
                }
            }

            return true;
        }

        public bool Equals(DiagnosticResult other)
        {
            return Equals(locations, other.locations) && Severity == other.Severity && string.Equals(Id, other.Id) && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DiagnosticResult && Equals((DiagnosticResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (locations != null ? locations.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Severity;
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}