namespace Simple.SemanticVersion;

public abstract class SemanticVersionComparerAbstract : IComparer<SemanticVersion>, IEqualityComparer<SemanticVersion> {
  public abstract int Compare(SemanticVersion? left, SemanticVersion? right);

  public virtual bool Equals(SemanticVersion? left, SemanticVersion? right) {
    return Compare(left, right) == 0;
  }

  public virtual int GetHashCode(SemanticVersion? value) {
    if (value is null) {
      return 0;
    }

    var result = 0;

    for (var i = Math.Min(value.Release.Count, 3) - 1; i >= 0; --i) {
      result = HashCode.Combine(result, value.Release[i]);
    }

    return result;
  }

  protected static int CompareItems(string? left, string? right) {
    left = string.IsNullOrWhiteSpace(left) ? "0" : left;
    right = string.IsNullOrWhiteSpace(right) ? "0" : right;

    var allDigitsLeft = left.Length > 0 && left.All(c => c >= '0' && c <= '9');
    var allDigitsRight = right.Length > 0 && right.All(c => c >= '0' && c <= '9');

    if (allDigitsLeft) {
      if (allDigitsRight) {
        var compare = left.Length.CompareTo(right.Length);

        return compare != 0
            ? compare
            : string.CompareOrdinal(left, right);
      }

      return -1;
    }

    if (allDigitsRight) {
      return 1;
    }

    var result = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);

    return result == 0
        ? string.Compare(left, right, StringComparison.Ordinal)
        : result;
  }
}