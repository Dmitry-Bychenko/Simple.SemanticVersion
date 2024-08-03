namespace Simple.SemanticVersion.Tests;

public sealed class SemanticVersionComparerTest {
  public static readonly TheoryData<string> VersionsToCompare = new()
  {
        "1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta+789 < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0 < 1.0.1 < 1.1.0 < 1.1.3 < 2.0.0",
        "3.0.4 < 3.0.5-alpha < 3.0.5 < 3.1.0 < 3.1.1-alpha < 3.1.1-beta"
    };

  [Theory]
  [MemberData(nameof(VersionsToCompare))]
  public void Full_Sorted(string text) {
    // Arrange
    var expected = string.Join(" < ", text
        .Split('>')
        .Select(s => s.Trim()));

    var random = new Random(123);

    // Act
    var actual = string.Join(" < ", text
        .Split('<')
        .OrderBy(_ => random.NextDouble())
        .Select(s => SemanticVersion.Parse(s))
        .OrderBy(v => v, SemanticVersionComparer.Full)
        .Select(v => v.ToString()));

    // Assert
    Assert.Equal(expected, actual);
  }
}