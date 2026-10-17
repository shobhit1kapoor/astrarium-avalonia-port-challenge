using System.Text.Json;

namespace Astrarium.Port.Core;

/// <summary>A curated, offline list of bright NGC/IC and related deep-sky targets.</summary>
public sealed class DeepSkyCatalog
{
    private const string ResourceName = "Astrarium.Port.Core.Data.deepsky.json";
    private static readonly Lazy<DeepSkyCatalog> Bundled = new(LoadBundledCore);
    private readonly IReadOnlyList<DeepSkyCatalogEntry> _objects;

    private DeepSkyCatalog(DeepSkyCatalogEntry[] objects)
    {
        _objects = Array.AsReadOnly(objects);
    }

    public IReadOnlyList<DeepSkyCatalogEntry> Objects => _objects;

    public static DeepSkyCatalog LoadBundled() => Bundled.Value;

    public static DeepSkyCatalog Load(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var objects = JsonSerializer.Deserialize(source, AstrariumJsonContext.Default.DeepSkyCatalogEntryArray);
        if (objects is null || objects.Length == 0)
            throw new InvalidDataException("The bundled deep-sky catalog is empty or invalid.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in objects)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id) ||
                string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.CatalogName) ||
                string.IsNullOrWhiteSpace(item.Designation) || string.IsNullOrWhiteSpace(item.ObjectType) ||
                !double.IsFinite(item.RightAscensionDegreesJ2000) ||
                item.RightAscensionDegreesJ2000 is < 0 or >= 360 ||
                !double.IsFinite(item.DeclinationDegreesJ2000) ||
                item.DeclinationDegreesJ2000 is < -90 or > 90 ||
                !double.IsFinite(item.Magnitude) ||
                !IsValidAxis(item.MajorAxisArcMinutes) || !IsValidAxis(item.MinorAxisArcMinutes))
            {
                throw new InvalidDataException($"The bundled deep-sky catalog contains an invalid object '{item?.Id ?? "<null>"}'.");
            }
        }

        return new DeepSkyCatalog(objects);
    }

    private static DeepSkyCatalog LoadBundledCore()
    {
        var resource = typeof(DeepSkyCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The bundled deep-sky catalog resource '{ResourceName}' was not found.");
        using (resource)
            return Load(resource);
    }

    private static bool IsValidAxis(double? axis) => axis is null || double.IsFinite(axis.Value) && axis.Value >= 0;
}

public sealed record DeepSkyCatalogEntry
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CatalogName { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string ObjectType { get; init; } = string.Empty;
    public double RightAscensionDegreesJ2000 { get; init; }
    public double DeclinationDegreesJ2000 { get; init; }
    public double Magnitude { get; init; }
    public double? MajorAxisArcMinutes { get; init; }
    public double? MinorAxisArcMinutes { get; init; }
    public string Constellation { get; init; } = string.Empty;
}
