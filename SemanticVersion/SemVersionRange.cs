using System;
using System.Text.RegularExpressions;

namespace SemanticVersion
{
    public class SemVersionRange
    {
        public SemVersion From { get; }
        public SemVersion To { get; }

        /*private static readonly Regex ParseRangeRegex =
            new Regex(@$"(?<=\s*\[?\s*(>=\s*)?)(?<from>{SemVersion.ParseEx})\s*-?\s*(<=\s*)?(?<to>{SemVersion.ParseEx})",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
                TimeSpan.FromSeconds(0.5));*/

        private static readonly Regex ParseRangeRegex =
            new Regex(@$"(?<=\s*\[?\s*(>=\s*)?)(?<from>{SemVersion.ParseEx})",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
                TimeSpan.FromSeconds(0.5));

        public SemVersionRange(SemVersion from, SemVersion to = null)
        {
            if (to != null && from > to) throw new ArgumentOutOfRangeException(nameof(from), "From оказался больше, чем To");

            From = from;
            To = to;
        }

        private static SemVersion SetSemVersionTo(SemVersion from)
        {
            if (from.Patch > 0)
                return new SemVersion(from.Major, from.Minor, from.Patch + 1, from.PreRelease, from.Build);
            else if (from.Minor > 0)
                return new SemVersion(from.Major, from.Minor + 1, from.Patch, from.PreRelease, from.Build);
            else if (from.Major > 0)
                return new SemVersion(from.Major + 1, from.Minor, from.Patch, from.PreRelease, from.Build);
            else
                return null;

        }

        public static SemVersionRange Parse(string source)
        {
            var match = ParseRangeRegex.Match(source);
            if (!match.Success)
                throw new ArgumentException($"Invalid range '{source}'.", nameof(source));


            var fromRange = SemVersion.Parse(match.Groups["from"].Value);
            var toRange = SetSemVersionTo(fromRange);

            return new SemVersionRange(fromRange, toRange);
        }

        public bool Contains(SemVersion version)
        {
            return version >= From && version < To || (To == null && version >= From);
        }

        public bool Contains(SemVersionRange range)
        {
            return Contains(range.From) && ( range.To != null ? Contains(range.To) : true);
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
            return $"[{From}{(To != null ? "-" + To : "")}]";
        }
    }
}
