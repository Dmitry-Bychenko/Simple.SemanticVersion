namespace Simple.SemanticVersion;

public static class SemanticVersionComparer
{
    #region Public Methods

    /// <summary>
    /// Compares Release, PreRelease and Metadata
    /// </summary>
    public static SemanticVersionComparerAbstract Full { get; } = new SemanticVersionComparerFull();

    /// <summary>
    /// Compares Release, PreRelease but not Metadata
    /// </summary>
    public static SemanticVersionComparerAbstract PreRelease { get; } = new SemanticVersionComparerPreRelease();

    /// <summary>
    /// Compares Release; version which contains PreRelease it's lower than version which doesn't have PreRelease
    /// </summary>
    public static SemanticVersionComparerAbstract Release { get; } = new SemanticVersionComparerRelease();

    /// <summary>
    /// Compares Release only
    /// </summary>
    public static SemanticVersionComparerAbstract ReleaseOnly { get; } = new SemanticVersionComparerReleaseOnly();

    /// <summary>
    /// Compares Release Major, Minor, Build - top three components - only
    /// </summary>
    public static SemanticVersionComparerAbstract ReleaseTopThree { get; } =
        new SemanticVersionComparerReleaseTopThree();

    /// <summary>
    /// Default comparison method
    /// </summary>
    public static SemanticVersionComparerAbstract Default { get; } = Full;

    /// <summary>
    /// Standard comparison method as it is described in the semantic version standard
    /// </summary>
    public static SemanticVersionComparerAbstract Standard { get; } = PreRelease;

    #endregion Public Methods
}