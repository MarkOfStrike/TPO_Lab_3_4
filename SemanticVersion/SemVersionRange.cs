using System;
using System.Text.RegularExpressions;

namespace SemanticVersion
{
    public class SemVersionRange
    {
        public SemVersion From { get; }
        public SemVersion To { get; }

        private static readonly Regex ParseRangeRegex =
            new Regex(@$"(?<=\s*\[?\s*(>=\s*)?)(?<from>{SemVersion.ParseEx})\s*-?\s*(<=\s*)?(?<to>{SemVersion.ParseEx})",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
                TimeSpan.FromSeconds(0.5));

        public SemVersionRange(SemVersion from, SemVersion to)
        {
            if (from > to) throw new ArgumentOutOfRangeException(nameof(from), "From оказался больше, чем To");

            From = from;
            To = to;
        }

        public static SemVersionRange Parse(string source)
        {
            var match = ParseRangeRegex.Match(source);
            if (!match.Success)
                throw new ArgumentException($"Invalid range '{source}'.", nameof(source));

            var fromRange = SemVersion.Parse(match.Groups["from"].Value, true);
            var toRange = SemVersion.Parse(match.Groups["to"].Value, true);

            return new SemVersionRange(fromRange, toRange);
        }

        public bool Contains(SemVersion version)
        {
            return version >= From && version <= To;
        }

        public bool Contains(SemVersionRange range)
        {
            return Contains(range.From) && Contains(range.To);
        }

        protected bool Equals(SemVersionRange other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemVersionRange)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To);
        }

        public override string ToString()
        {
            return $"[{From} - {To}]";
        }
    }
}
