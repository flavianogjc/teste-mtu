using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Mtu.Rentals.Tests.Infra;
using Xunit;

namespace Mtu.Rentals.Tests.Integration;

public sealed class CouriersApiTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public CouriersApiTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Create_Then_GetById_ShouldWork()
    {
        var body = new
        {
            nome = "  Ana da Silva  ",
            cnpj = "12345678000100",
            data_nascimento = new DateOnly(1990, 1, 1),
            numero_cnh = " ab-123.456 ",
            tipo_cnh = "A+B"
        };

        var postResp = await _client.PostAsJsonAsync("/entregadores", body, JsonOpts);
        if (!postResp.IsSuccessStatusCode)
        {
            var err = await postResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"POST /entregadores falhou: {(int)postResp.StatusCode} {postResp.StatusCode}\n{err}");
        }

        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResp.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        created.Should().NotBeNull();
        created!.nome.Should().Be("Ana da Silva");
        created.numero_cnh.Should().Be("AB123456");
        created.tipo_cnh.Should().Be("A+B");
        created.imagem_cnh.Should().BeNull();

        var id = created.identificador;
        var getResp = await _client.GetAsync($"/entregadores/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var got = await getResp.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        got!.nome.Should().Be("Ana da Silva");
        got.imagem_cnh.Should().BeNull();
    }

    [Fact]
    public async Task UploadCnhBase64_Then_GetById_ShouldReturnBase64()
    {
        // 1) create courier
        var post = await _client.PostAsJsonAsync("/entregadores", new
        {
            nome = "Mia",
            cnpj = "12345678000101",
            data_nascimento = new DateOnly(2000, 2, 2),
            numero_cnh = "C2",
            tipo_cnh = "AB"
        }, JsonOpts);

        if (!post.IsSuccessStatusCode)
        {
            var err = await post.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"POST /entregadores falhou: {(int)post.StatusCode} {post.StatusCode}\n{err}");
        }

        var created = await post.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        var id = created!.identificador;

        // 2) send base64 of a minimal PNG
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // signature PNG
        var base64 = Convert.ToBase64String(pngBytes);
        var req = new { imagem_cnh = $"data:image/png;base64,{base64}" }; // data-URI is accepted

        var put = await _client.PutAsJsonAsync($"/entregadores/{id}/cnh", req, JsonOpts);
        if (!put.IsSuccessStatusCode)
        {
            var err = await put.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"PUT /entregadores/{id}/cnh falhou: {(int)put.StatusCode} {put.StatusCode}\n{err}");
        }

        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterUpload = await put.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        afterUpload!.imagem_cnh.Should().NotBeNull();

        afterUpload.imagem_cnh.Should().Be(base64);

        // 3) GET by id must bring the same base64 (read from the saved file)
        var get = await _client.GetAsync($"/entregadores/{id}");
        get.EnsureSuccessStatusCode();

        var got = await get.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        got!.imagem_cnh.Should().Be(base64);
    }

    [Fact]
    public async Task GetById_NotFound_WhenGuidDoesNotExist()
    {
        var resp = await _client.GetAsync($"/entregadores/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadCnhBase64_ShouldReject_Jpeg()
    {
        // create courier
        var post = await _client.PostAsJsonAsync("/entregadores", new
        {
            nome = "Zara",
            cnpj = "12345678000102",
            data_nascimento = new DateOnly(1999, 9, 9),
            numero_cnh = "A1",
            tipo_cnh = "A"
        }, JsonOpts);
        post.EnsureSuccessStatusCode();

        var created = await post.Content.ReadFromJsonAsync<CourierPtBrResponse>(JsonOpts);
        var id = created!.identificador;

        // JPEG: 0xFF 0xD8 (SOI) 0xFF
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xDB };
        var base64Jpeg = Convert.ToBase64String(jpegHeader);
        var req = new { imagem_cnh = $"data:image/jpeg;base64,{base64Jpeg}" };

        var resp = await _client.PutAsJsonAsync($"/entregadores/{id}/cnh", req, JsonOpts);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await resp.Content.ReadAsStringAsync();
        json.Should().Contain("Dados inválidos");
    }
}

public sealed record CourierPtBrResponse(
    string identificador,
    string nome,
    string cnpj,
    string data_nascimento,
    string numero_cnh,
    string tipo_cnh,
    string? imagem_cnh
);
