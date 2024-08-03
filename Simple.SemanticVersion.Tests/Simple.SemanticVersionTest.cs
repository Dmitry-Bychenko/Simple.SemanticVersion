namespace Simple.SemanticVersion.Tests;

public sealed class SemanticVersionTest {
  [Theory]
  [InlineData("1.0.0")]
  [InlineData("2.3.5")]
  [InlineData("2.0.5")]
  [InlineData("2.7.0")]
  [InlineData("12.003.15")]
  [InlineData("2.a.0")]
  [InlineData("2.a.")]
  [InlineData("2..0")]
  [InlineData("2..")]
  [InlineData("...")]
  [InlineData("a.b.0")]
  [InlineData("a.00b.0")]
  [InlineData("a.00b.00c")]
  [InlineData("0a.00b.000c")]
  public static void Parse_ReleaseVersion_Parsed(string text) {
    // Arrange
    var expected = text
        .Split('.')
        .Select(item => item.Trim())
        .Select(item => item.All(c => c is >= '0' and < '9') ? item.TrimStart('0') : item)
        .Reverse()
        .SkipWhile(string.IsNullOrEmpty)
        .Reverse()
        .Select(item => string.IsNullOrEmpty(item) ? "0" : item)
        .ToList();

    // Act
    var actual = SemanticVersion.Parse(text);

    // Assert
    Assert.True(expected.SequenceEqual(actual.Release),
        $"expected: {string.Join(".", expected)} vs. actual: {string.Join(".", actual.Release)}");
  }
}