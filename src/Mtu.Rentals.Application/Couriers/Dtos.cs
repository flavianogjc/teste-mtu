// src/Mtu.Rentals.Application/Couriers/Dtos.cs
using System;
using System.Text.Json.Serialization;

public sealed record CreateCourierPtBrRequest(
    [property: JsonPropertyName("identificador")]     string Identificador,
    [property: JsonPropertyName("nome")]              string Nome,
    [property: JsonPropertyName("cnpj")]              string Cnpj,
    [property: JsonPropertyName("data_nascimento")]   string DataNascimento, // "YYYY-MM-DDTHH:mm:ssZ" (UTC)
    [property: JsonPropertyName("numero_cnh")]        string NumeroCnh,
    [property: JsonPropertyName("tipo_cnh")]          string TipoCnh,       // "A" | "B" | "A+B"
    [property: JsonPropertyName("imagem_cnh")]        string? ImagemCnh     // base64 ou null
);

public sealed record CourierPtBrResponse(
    [property: JsonPropertyName("id")]            Guid   Id,
    [property: JsonPropertyName("identificador")]
    string Identificador,
    [property: JsonPropertyName("nome")]
    string Nome,
    [property: JsonPropertyName("cnpj")]
    string Cnpj,
    [property: JsonPropertyName("data_nascimento")]
    string DataNascimento, // "YYYY-MM-DDTHH:mm:ssZ" (UTC)
    [property: JsonPropertyName("numero_cnh")]
    string NumeroCnh,
    [property: JsonPropertyName("tipo_cnh")]
    string TipoCnh,        // "A" | "B" | "A+B"
    [property: JsonPropertyName("imagem_cnh")]
    string? ImagemCnh      // base64 ou null
);

// REQUEST para upload via base64
public sealed record UploadCnhBase64Request(
    [property: JsonPropertyName("imagem_cnh")]
    string ImagemCnhBase64
);
