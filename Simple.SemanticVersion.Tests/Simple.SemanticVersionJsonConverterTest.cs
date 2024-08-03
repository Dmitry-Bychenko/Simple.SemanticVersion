using System.Text.Json;

namespace Simple.SemanticVersion.Tests;

public sealed class JsonConverterTest {
  [Theory]
  [InlineData("1.0.0")]
  [InlineData("1..0")]
  public void Serialization_Serialized(string text) {
    // Arrange
    var version = SemanticVersion.Parse(text);

    // Act
    var json = JsonSerializer.Serialize(version);

    // Assert
    var expected = "\"" + version.ToString().Replace("\"", "\"\"") + "\"";

    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData("1.0.0")]
  [InlineData("1..0")]
  public void Deserialization_Deserialized(string text) {
    // Arrange
    var json = "\"" + text.Replace("\"", "\"\"") + "\"";

    // Act
    var version = JsonSerializer.Deserialize<SemanticVersion>(json);

    // Assert
    var expected = SemanticVersion.Parse(text);

    Assert.Equal(expected, version);
  }
}