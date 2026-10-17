using System.Text.Json;

namespace Astrarium.Port.Core;

public sealed class StarCatalog
{
    private readonly StarCatalogEntry[] _stars;

    private StarCatalog(StarCatalogEntry[] stars)
    {
        _stars = stars;
    }

    public IReadOnlyList<StarCatalogEntry> Stars => _stars;

    public static StarCatalog LoadBundled()
    {
        const string resourceName = "Astrarium.Port.Core.Data.stars.json";
        var resource = typeof(StarCatalog).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The bundled star catalog resource '{resourceName}' was not found.");
        using (resource)
            return Load(resource);
    }

    public static StarCatalog Load(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var stars = JsonSerializer.Deserialize<StarCatalogEntry[]>(source, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (stars is null || stars.Length == 0)
            throw new InvalidDataException("The bundled star catalog is empty or invalid.");

        return new StarCatalog(stars);
    }
}

public sealed record StarCatalogEntry
{
    public string Id { get; init; } = string.Empty;
    public string Hip { get; init; } = string.Empty;
    public string Hr { get; init; } = string.Empty;
    public string Hd { get; init; } = string.Empty;
    public string Proper { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string Constellation { get; init; } = string.Empty;
    public double RaDegreesJ2000 { get; init; }
    public double DecDegreesJ2000 { get; init; }
    public double Magnitude { get; init; }
    public double? ColorIndex { get; init; }
    public double[] PositionParsecsJ2000 { get; init; } = [];
    public double[] VelocityParsecsPerYear { get; init; } = [];

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Proper) ? Proper :
        !string.IsNullOrWhiteSpace(Designation) ? Designation :
        !string.IsNullOrWhiteSpace(Hr) ? $"HR {Hr}" : $"HYG {Id}";
}
