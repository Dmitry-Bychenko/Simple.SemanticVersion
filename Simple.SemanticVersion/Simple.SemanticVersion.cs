using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Simple.SemanticVersion;

/// <summary>
/// Semantic version
/// </summary>
/// <seealso cref="https://semver.org/"/>
[JsonConverter(typeof(SemanticVersionJsonConverter))]
public sealed class SemanticVersion :
    ISpanParsable<SemanticVersion>,
    IEquatable<SemanticVersion?>,
    IComparable<SemanticVersion?>,
    ISpanFormattable
{
    #region Private Fields and Properties 

    public static readonly IReadOnlyList<string> KnownPrefixes = [
        "v.",
        "ver.",
        "version.",
    ];

    #endregion Private Fields and Properties 

    #region Public Properties

    public static SemanticVersion None { get; } = new();

    public IReadOnlyList<string> Release { get; private init; } = [];

    public IReadOnlyList<string> PreRelease { get; private init; } = [];

    public string Metadata { get; private init; } = string.Empty;

    public string Major => Release.Count > 0 ? Release[0] : "0";

    public string Minor => Release.Count > 1 ? Release[1] : "0";

    public string Patch => Release.Count > 2 ? Release[2] : "0";

    public ReleaseKind Kind
    {
        get
        {
            if (PreRelease.Count <= 0)
                return ReleaseKind.Release;

            var text = PreRelease[0];

            if (text.Contains("rc", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("candidate", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("releasecandidate", StringComparison.OrdinalIgnoreCase))
                return ReleaseKind.ReleaseCandidate;

            if (text.Contains("beta", StringComparison.OrdinalIgnoreCase))
               return ReleaseKind.Beta;

            if (text.Contains("alpha", StringComparison.OrdinalIgnoreCase))
                return ReleaseKind.Alpha;

            return ReleaseKind.Unknown;
        }
    }

    public bool IsWellFormed => Release.Count <= 3 &&
                                Release.All(item => item.All(c => c is >= '0' and <= '9')) &&
                                PreRelease.All(item =>
                                    item.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z')) &&
                                Metadata.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z');
    
    #endregion Public Properties

    #region Create

    private SemanticVersion()
    {
    }

    public SemanticVersion(long major) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);

        Release = CompressList([
            major.ToString(CultureInfo.InvariantCulture)
        ]);
    }

    public SemanticVersion(long major, long minor) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);

        Release = CompressList([
            major.ToString(CultureInfo.InvariantCulture),
            minor.ToString(CultureInfo.InvariantCulture)
        ]);
    }

    public SemanticVersion(long major, long minor, long patch) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);

        Release = CompressList([
            major.ToString(CultureInfo.InvariantCulture),
            minor.ToString(CultureInfo.InvariantCulture),
            patch.ToString(CultureInfo.InvariantCulture)
        ]);
    }

    public SemanticVersion(long major, long minor, long patch, long build) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);
        ArgumentOutOfRangeException.ThrowIfNegative(build);

        Release = CompressList([
            major.ToString(CultureInfo.InvariantCulture),
            minor.ToString(CultureInfo.InvariantCulture),
            patch.ToString(CultureInfo.InvariantCulture),
            build.ToString(CultureInfo.InvariantCulture)
        ]);
    }

    public SemanticVersion(IEnumerable<string?> release, IEnumerable<string?>? preRelease = default, string? metadata = default)
    {
        ArgumentNullException.ThrowIfNull(release);

        Release = CompressList(release
            .Select(item => item ?? string.Empty)
            .Select(item => TrimItem(item.AsSpan()))
            .ToList());

        PreRelease = preRelease is null 
            ? [] 
            : CompressList(preRelease
                .Select(item => item ?? string.Empty)
                .Select(item => TrimItem(item.AsSpan()))
                .ToList());

        Metadata = string.IsNullOrWhiteSpace(metadata) 
            ? string.Empty 
            : metadata.Trim();
    }

    public SemanticVersion(Version version) : this()
    {
        ArgumentNullException.ThrowIfNull(version);

        Release = CompressList([
            Math.Clamp(version.Major, 0, int.MaxValue).ToString(CultureInfo.InvariantCulture),
            Math.Clamp(version.Minor, 0, int.MaxValue).ToString(CultureInfo.InvariantCulture),
            Math.Clamp(version.Build, 0, int.MaxValue).ToString(CultureInfo.InvariantCulture),
            Math.Clamp(version.Revision, 0, int.MaxValue).ToString(CultureInfo.InvariantCulture)
        ]);
    }

    #endregion Create

    #region Operators

    #region Comparison

    public static bool operator ==(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) == 0;

    public static bool operator !=(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) != 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) >= 0;

    public static bool operator <=(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) <= 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) > 0;

    public static bool operator <(SemanticVersion left, SemanticVersion right) => SemanticVersionComparer.Default.Compare(left, right) < 0;

    #endregion Comparison

    #region Cast

    public static implicit operator SemanticVersion(Version? version)
    {
        return version is null
            ? None
            : new SemanticVersion(version);
    }

    #endregion Cast

    #endregion Operators

    #region ISpanParsable<SemanticVersion>

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out SemanticVersion result)
    {
        result = None;

        s = RemovePrefixes(s).Trim();

        if (s.Length == 0)
        {
            return false;
        }

        List<string> release = [];
        List<string> preRelease = [];
        List<string> list = release;

        var metadata = string.Empty;

        for (int i = 0, left = 0; i <= s.Length; ++i)
        {
            if (i == s.Length)
            {
                list.Add(TrimItem(s[left .. i]));
            }
            else if (s[i] == '-' && ReferenceEquals(list, release))
            {
                list.Add(TrimItem(s[left..i]));

                list = preRelease;

                left = i + 1;
            }
            else if (s[i] == '+')
            {
                list.Add(TrimItem(s[left..i]));
                metadata = s[(i + 1)..].Trim().ToString();

                break;
            }
            else if (s[i] == '.')
            {
                list.Add(TrimItem(s[left..i]));

                left = i + 1;
            }
        }

        result = new SemanticVersion
        {
            Release = CompressList(release),
            PreRelease = CompressList(preRelease),
            Metadata = metadata.Trim()
        };

        return true;
    }

    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out SemanticVersion result) =>
        TryParse(s, default, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out SemanticVersion result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out SemanticVersion result) =>
        TryParse(s, default, out result);

    public static SemanticVersion Parse(ReadOnlySpan<char> s, IFormatProvider? provider = default)
    {
        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException("Not a valid semantic version");
    }

    public static SemanticVersion Parse(string? s, IFormatProvider? provider = default)
    {
        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException("Not a valid semantic version");
    }

    #endregion ISpanParsable<SemanticVersion>

    #region IEquatable<SemanticVersion>

    public bool Equals(SemanticVersion? other) => SemanticVersionComparer.Default.Equals(this, other);

    public override bool Equals(object? obj) => Equals(obj as SemanticVersion);

    public override int GetHashCode()
    {
        return SemanticVersionComparer.Default.GetHashCode(this);
    }

    #endregion IEquatable<SemanticVersion>

    #region IComparable<SemanticVersion>

    public int CompareTo(SemanticVersion? other) => SemanticVersionComparer.Default.Compare(this, other);

    #endregion IComparable<SemanticVersion>

    #region ISpanFormattable

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = default)
    {
        var result = ToString(format.ToString(), provider);

        if (result.Length > destination.Length)
        {
            charsWritten = 0;

            return false;
        }

        result.CopyTo(destination);

        charsWritten = result.Length;

        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider = default)
    {
        format = string.IsNullOrWhiteSpace(format) ? "RPM" : format;

        StringBuilder sb = new();

        if (format.Contains('R', StringComparison.OrdinalIgnoreCase))
        {
            sb.Append(Release.Count switch {
                0 => "0.0.0",
                1 => $"{Release[0]}.0.0",
                2 => $"{Release[0]}.{Release[1]}.0",
                _ => string.Join(".", Release)
            });
        }

        if (format.Contains('P', StringComparison.OrdinalIgnoreCase) && PreRelease.Count > 0)
        {
            if (sb.Length > 0)
                sb.Append('-');

            sb.Append(string.Join(".", PreRelease));
        }

        if (format.Contains('M', StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Metadata))
        {
            if (sb.Length > 0)
              sb.Append('+');

            sb.Append(Metadata);
        }

        return sb.ToString();
    }

    public override string ToString()
    {
        return ToString(default);
    }

    #endregion ISpanFormattable

    #region Private Methods

    private static string TrimItem(ReadOnlySpan<char> value)
    {
        var text = value.Trim().ToString();

        if (text.Length > 0 && text.All(c => c is >= '0' and <= '9'))
        {
            text = text.TrimStart('0').Trim();

            return string.IsNullOrEmpty(text) ? "0" : text;
        }

        return text;
    }

    private static List<string> CompressList(List<string> list)
    {
        for (var i = list.Count - 1; i >= 0; --i)
        {
            if (!"0".Equals(list[i]) && !string.IsNullOrEmpty(list[i]))
            {
                list.RemoveRange(i + 1, list.Count - i - 1);

                return list;
            }
        }

        return [];
    }

    private static ReadOnlySpan<char> RemovePrefixes(ReadOnlySpan<char> text)
    {
        foreach (var prefix in KnownPrefixes)
        {
            if (text.StartsWith(prefix) && text.Length > prefix.Length && char.IsWhiteSpace(text[prefix.Length]))
            {
                return text[.. prefix.Length];
            }
        }

        return text;
    }

    #endregion Private Methods
}
