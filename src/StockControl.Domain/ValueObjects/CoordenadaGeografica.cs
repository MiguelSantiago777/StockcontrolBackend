using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class CoordenadaGeografica : ValueObject
{
    private CoordenadaGeografica(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public static Result<CoordenadaGeografica> Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            return Result.Failure<CoordenadaGeografica>(Error.Validation("Coordenada.Latitude", "Latitude inválida."));
        if (longitude is < -180 or > 180)
            return Result.Failure<CoordenadaGeografica>(Error.Validation("Coordenada.Longitude", "Longitude inválida."));
        return new CoordenadaGeografica(latitude, longitude);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
