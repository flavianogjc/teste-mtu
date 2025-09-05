using System.Text.Json.Serialization;

namespace Mtu.Rentals.Application.Motorcycles;

public sealed record CreateMotorcycleRequest(
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("ano")]          int    Year,
    [property: JsonPropertyName("modelo")]       string Model,
    [property: JsonPropertyName("placa")]        string Plate);

public sealed record MotorcycleResponse(
    [property: JsonPropertyName("id")]            Guid   Id,
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("ano")]           int    Year,
    [property: JsonPropertyName("modelo")]        string Model,
    [property: JsonPropertyName("placa")]         string Plate);

public sealed record UpdatePlateRequest(
    [property: JsonPropertyName("placa")] string Plate);
