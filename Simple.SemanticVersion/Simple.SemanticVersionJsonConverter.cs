using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simple.SemanticVersion;

public sealed class SemanticVersionJsonConverter : JsonConverter<SemanticVersion> {
  public override SemanticVersion Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options) {
    return SemanticVersion.Parse(reader.GetString(), CultureInfo.InvariantCulture);
  }

  public override void Write(
      Utf8JsonWriter writer,
      SemanticVersion value,
      JsonSerializerOptions options) {
    writer.WriteStringValue(value.ToString());
  }
}