using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticVersion
{
    /// <summary>
    /// Представляет собой модель семантической версии.
    /// </summary>
    public class SemVersion
    {
        /// <summary>
        /// Получает МАЖОРНУЮ версию.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// Получает МИНОРНУЮ версию.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// Получает ПАТЧ-версию.
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// Получает пререлизную версию.
        /// </summary>
        public string PreRelease { get; }

        /// <summary>
        /// Получает билд версию.
        /// </summary>
        public string Build { get; }


        public static readonly Regex ParseEx =
            new Regex(@"(?<major>(\d|\*?|x?|X?|^\s?$)+)" +
                      @"(?>\.(?<minor>(\d|\*?|x?|X?|^\s?$)+))?" +
                      @"(?>\.(?<patch>(\d|\*?|x?|X?|^\s?$)+))?" +
                      @"(?>\-(?<pre>[0-9A-Za-z\-\.]+))?" +
                      @"(?>\+(?<build>[0-9A-Za-z\-\.]+))?",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
                TimeSpan.FromSeconds(0.5));

        public SemVersion(int major, int minor = 0, int patch = 0, string preRelease = "", string build = "")
        {
            Major = major;
            Minor = minor;
            Patch = patch;

            PreRelease = preRelease ?? "";
            Build = build ?? "";
        }

        /// <summary>
        /// Разбирает указанную строку в модель семантического версионирования.
        /// </summary>
        /// <param name="version">Строка версии.</param>
        /// <param name="strict">Признак того, что выполняется валидация указанного строки версии.</param>
        /// <exception cref="InvalidOperationException">Ошибка, возникающая в случае невалидной строки при выполнении валидации.</exception>
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = ParseEx.Match(version);
            if (!match.Success)
                throw new ArgumentException($"Invalid version '{version}'.", nameof(version));

            int.TryParse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int major);

            

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
                int.TryParse(minorMatch.Value,NumberStyles.Integer, CultureInfo.InvariantCulture, out minor);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no minor version given in strict mode)");

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
                int.TryParse(patchMatch.Value,NumberStyles.Integer, CultureInfo.InvariantCulture, out patch);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no patch version given in strict mode)");

            var preRelease = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;

            return new SemVersion(major, minor, patch, preRelease, build);
        }

        protected bool Equals(SemVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch && PreRelease == other.PreRelease &&
                   Build == other.Build;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemVersion)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, PreRelease, Build);
        }

        public static bool operator ==(SemVersion ver1, SemVersion ver2)
        {
            if (ReferenceEquals(ver1, ver2))
            {
                return true;
            }

            if (ReferenceEquals(ver1, null))
            {
                return false;
            }

            if (ReferenceEquals(ver2, null))
            {
                return false;
            }

            return ver1.Equals(ver2);
        }

        public static bool operator !=(SemVersion ver1, SemVersion ver2)
        {
            return !(ver1 == ver2);
        }

        public static bool operator >(SemVersion ver1, SemVersion ver2)
        {
            if (ReferenceEquals(ver1, ver2)) return false;
            if (ver1 is null) return false;
            if (ver2 is null) return true;
            return Compare(ver1, ver2) > 0;
        }

        public static bool operator <(SemVersion ver1, SemVersion ver2)
        {
            if (ReferenceEquals(ver1, ver2)) return false;
            if (ver1 is null) return true;
            if (ver2 is null) return false;
            return Compare(ver1, ver2) < 0;
        }

        public static bool operator >=(SemVersion ver1, SemVersion ver2)
        {
            return ver1 > ver2 || ver1 == ver2;
        }

        public static bool operator <=(SemVersion ver1, SemVersion ver2)
        {
            return ver1 < ver2 || ver1 == ver2;
        }

        public static int Compare(SemVersion that, SemVersion other)
        {
            var r = CompareByPrecedence(that, other);
            if (r != 0) return r;

            return CompareComponent(that.Build, other.Build);
        }

        public static int CompareByPrecedence(SemVersion that, SemVersion other)
        {
            var r = that.Major.CompareTo(other.Major);
            if (r != 0) return r;

            r = that.Minor.CompareTo(other.Minor);
            if (r != 0) return r;

            r = that.Patch.CompareTo(other.Patch);
            if (r != 0) return r;

            return CompareComponent(that.PreRelease, other.PreRelease, true);
        }

        private static int CompareComponent(string a, string b, bool nonemptyIsLower = false)
        {
            var aEmpty = string.IsNullOrEmpty(a);
            var bEmpty = string.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return nonemptyIsLower ? 1 : -1;
            if (bEmpty)
                return nonemptyIsLower ? -1 : 1;

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                var aIsNum = int.TryParse(ac, out var aNum);
                var bIsNum = int.TryParse(bc, out var bNum);
                int r;
                if (aIsNum && bIsNum)
                {
                    r = aNum.CompareTo(bNum);
                    if (r != 0) return r;
                }
                else
                {
                    if (aIsNum)
                        return -1;
                    if (bIsNum)
                        return 1;
                    r = string.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }

            return aComps.Length.CompareTo(bComps.Length);
        }

        /// <summary>
        /// Получает текущую версию в строковом представлении.
        /// </summary>
        public override string ToString()
        {
            var estimatedLength = 4 + Major.Digits() + Minor.Digits() + Patch.Digits()
                                  + PreRelease.Length + Build.Length;
            var version = new StringBuilder(estimatedLength);
            version.Append(Major);
            version.Append('.');
            version.Append(Minor);
            version.Append('.');
            version.Append(Patch);
            if (PreRelease.Length > 0)
            {
                version.Append('-');
                version.Append(PreRelease);
            }

            if (Build.Length > 0)
            {
                version.Append('+');
                version.Append(Build);
            }

            return version.ToString();
        }
    }
}
