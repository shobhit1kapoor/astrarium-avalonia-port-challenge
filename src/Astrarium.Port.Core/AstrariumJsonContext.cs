using System.Text.Json.Serialization;

namespace Astrarium.Port.Core;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
[JsonSerializable(typeof(StarCatalogEntry[]))]
[JsonSerializable(typeof(UserPreferences))]
internal partial class AstrariumJsonContext : JsonSerializerContext
{
}
