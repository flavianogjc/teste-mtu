using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mtu.Rentals.Application;
using Mtu.Rentals.Infrastructure.Persistence;
using Mtu.Rentals.Domain.Entities;

[ApiController]
[Route("entregadores")]
public sealed class CouriersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _storage;
    public CouriersController(AppDbContext db, IFileStorage storage)
    { _db = db; _storage = storage; }

    [HttpPost]
    public async Task<ActionResult<CourierPtBrResponse>> Create(
        [FromBody] CreateCourierPtBrRequest req,
        CancellationToken ct)
    {
        // Parse direto de "1990-01-01T00:00:00Z" (ou com 'Z') sem imports extras
        DateTime birthUtc;
        try
        {
            birthUtc = DateTimeOffset.Parse(req.DataNascimento).UtcDateTime;
            // se seu domínio usa DateOnly:
            // var birthDateOnly = DateOnly.FromDateTime(birthUtc.Date);
            // e passe birthDateOnly no construtor
        }
        catch
        {
            return BadRequest(new { message = "data_nascimento_invalida" });
        }

        var c = new Courier(
            req.Identificador,
            req.Nome,
            new Cnpj(req.Cnpj),
            birthUtc.Date, // ou use birthUtc se seu domínio for DateTime
            req.NumeroCnh,
            ParseCnh(req.TipoCnh)
        );

        _db.Couriers.Add(c);

        if (!string.IsNullOrWhiteSpace(req.ImagemCnh))
        {
            try
            {
                var (bytes, ext) = DecodeBase64AndGetExt(req.ImagemCnh);
                if (ext is not ".png" and not ".bmp")
                    return BadRequest(new { message = "Dados inválidos", allowed = new[] { "png", "bmp" } });

                var filename = $"cnh_{c.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                await using var ms = new MemoryStream(bytes);
                var path = await _storage.SaveAsync(ms, filename, ct);
                c.SetCnhImage(path);
            }
            catch
            {
                return BadRequest(new { message = "invalid_base64" });
            }
        }

        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, ToPtBrDto(c));
    }



    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourierPtBrResponse>> GetById(Guid id, CancellationToken ct)
    {
        var c = await _db.Couriers.FindAsync(new object?[] { id }, ct);
        return c is null ? NotFound() : ToPtBrDto(c);
    }

    // ===== Upload BASE64 (JSON) =====
    // body: { "imagem_cnh": "<base64>" }
    [HttpPut("{id:guid}/cnh")]
    public async Task<ActionResult<CourierPtBrResponse>> UploadCnhBase64(Guid id, [FromBody] UploadCnhBase64Request body, CancellationToken ct)
    {
        var c = await _db.Couriers.FindAsync(new object?[] { id }, ct);
        if (c is null) return NotFound();

        if (string.IsNullOrWhiteSpace(body.ImagemCnhBase64))
            return BadRequest(new { message = "imagem_cnh_required" });

        byte[] bytes;
        string ext;
        try
        {
            (bytes, ext) = DecodeBase64AndGetExt(body.ImagemCnhBase64);
            if (ext is not ".png" and not ".bmp")
                return BadRequest(new { message = "Dados inválidos", allowed = new[] { "png", "bmp" } });
        }
        catch
        {
            return BadRequest(new { message = "invalid_base64" });
        }

        var filename = $"cnh_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        await using var ms = new MemoryStream(bytes);
        var path = await _storage.SaveAsync(ms, filename, ct);

        c.SetCnhImage(path);
        await _db.SaveChangesAsync(ct);

        return ToPtBrDto(c);
    }

    // ===== Helpers =====
    private static CourierPtBrResponse ToPtBrDto(Courier c)
    {
        // "YYYY-MM-DDTHH:mm:ssZ"
        var dtUtc = new DateTime(c.BirthDate.Year, c.BirthDate.Month, c.BirthDate.Day, 0, 0, 0, DateTimeKind.Utc);
        var dataNascimento = dtUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

        string? base64 = EncodeBase64FromPath(c.CnhImagePath);

        return new CourierPtBrResponse(
            Id: c.Id,
            Identificador: c.Identifier,
            Nome: c.Name,
            Cnpj: c.Cnpj.Digits,
            DataNascimento: dataNascimento,
            NumeroCnh: c.CnhNumber,
            TipoCnh: CnhToString(c.CnhType),
            ImagemCnh: base64
        );
    }

    private static string CnhToString(CnhType t) => t switch
    {
        CnhType.A  => "A",
        CnhType.B  => "B",
        CnhType.AB => "A+B",
        _          => ""
    };

    private static string? EncodeBase64FromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return null;
        var bytes = System.IO.File.ReadAllBytes(path);
        return Convert.ToBase64String(bytes);
    }

    private static (byte[] bytes, string ext) DecodeBase64AndGetExt(string base64)
    {
        var commaIdx = base64.IndexOf(',');
        if (commaIdx > -1) base64 = base64[(commaIdx + 1)..];

        var bytes = Convert.FromBase64String(base64);

        static bool HasPrefix(byte[] b, params byte[] sig) => b.Length >= sig.Length && !sig.Where((t, i) => b[i] != t).Any();

        var isPng = bytes.Length >= 8 && HasPrefix(bytes, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);
        var isBmp = bytes.Length >= 2 && bytes[0] == (byte)'B' && bytes[1] == (byte)'M';

        var ext = isPng ? ".png" : isBmp ? ".bmp" : ".bin";
        return (bytes, ext);
    }

    private static CnhType ParseCnh(string v) => v.ToUpperInvariant() switch
    {
        "A" => CnhType.A,
        "B" => CnhType.B,
        "A+B" or "AB" => CnhType.AB,
        _ => CnhType.None
    };
}
