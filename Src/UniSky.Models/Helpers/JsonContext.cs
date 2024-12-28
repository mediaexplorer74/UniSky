using System.Text.Json.Serialization;
using UniSky.Models;

namespace UniSky.Helpers;

[JsonSerializable(typeof(SessionModel))]
[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class JsonContext : JsonSerializerContext
{
}
