namespace Simple.SemanticVersion;

public sealed class SemanticVersionComparerPreRelease : SemanticVersionComparerAbstract
{
    public override int Compare(SemanticVersion? left, SemanticVersion? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        for (var i = 0; i < Math.Max(left.Release.Count, right.Release.Count); ++i)
        {
            var leftItem = i < left.Release.Count ? left.Release[i] : null;
            var rightItem = i < right.Release.Count ? right.Release[i] : null;

            var compare = CompareItems(leftItem, rightItem);

            if (compare != 0)
            {
                return compare;
            }
        }

        if (left.PreRelease.Count == 0 && right.PreRelease.Count > 0)
        {
            return +1;
        }

        if (right.PreRelease.Count == 0 && left.PreRelease.Count > 0)
        {
            return -1;
        }

        for (var i = 0; i < Math.Max(left.PreRelease.Count, right.PreRelease.Count); ++i)
        {
            var leftItem = i < left.PreRelease.Count ? left.PreRelease[i] : null;
            var rightItem = i < right.PreRelease.Count ? right.PreRelease[i] : null;

            var compare = CompareItems(leftItem, rightItem);

            if (compare != 0)
            {
                return compare;
            }
        }

        return 0;
    }
}