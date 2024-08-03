namespace Simple.SemanticVersion;

public sealed class SemanticVersionComparerReleaseTopThree : SemanticVersionComparerAbstract
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

        for (var i = 0; i < 3; ++i)
        {
            var leftItem = i < left.Release.Count ? left.Release[i] : null;
            var rightItem = i < right.Release.Count ? right.Release[i] : null;

            var compare = CompareItems(leftItem, rightItem);

            if (compare != 0)
            {
                return compare;
            }
        }

        return 0;
    }
}