using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mtu.Rentals.Application.Motorcycles;
using Mtu.Rentals.Contracts;
using Mtu.Rentals.Domain.Entities;
using Mtu.Rentals.Infrastructure.Persistence;

[ApiController]
[Route("motos")]
public sealed class MotorcyclesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _bus;

    public MotorcyclesController(AppDbContext db, IPublishEndpoint bus)
    {
        _db = db;
        _bus = bus;
    }

    [HttpPost]
    public async Task<ActionResult<MotorcycleResponse>> Create(
        [FromBody] CreateMotorcycleRequest req, CancellationToken ct)
    {
        var entity = new Motorcycle(req.Year, req.Model, new LicensePlate(req.Plate));

        _db.Motorcycles.Add(entity);
        await _db.SaveChangesAsync(ct);

        await _bus.Publish<IMotorcycleRegistered>(new
        {
            MotorcycleId = entity.Id,
            Year = entity.Year,
            Model = entity.Model
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            new MotorcycleResponse(
                entity.Id.ToString(),
                entity.Year,
                entity.Model,
                entity.Plate.Value
            ));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MotorcycleResponse>>> List(
        [FromQuery(Name = "placa")] string? plate,
        CancellationToken ct)
    {
        var q = _db.Motorcycles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(plate))
        {
            // comparação exata com o valor armazenado
            q = q.Where(m => m.Plate.Value == plate);
        }

        var items = await q
            .Select(m => new MotorcycleResponse(
                m.Id.ToString(),
                m.Year,
                m.Model,
                m.Plate.Value))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MotorcycleResponse>> GetById(Guid id, CancellationToken ct)
    {
        var m = await _db.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        return new MotorcycleResponse(
            m.Id.ToString(),
            m.Year,
            m.Model,
            m.Plate.Value
        );
    }

    [HttpPatch("{id:guid}/placa")]
    public async Task<ActionResult<MotorcycleResponse>> UpdatePlate(
        Guid id, [FromBody] UpdatePlateRequest req, CancellationToken ct)
    {
        var m = await _db.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        m.UpdatePlate(new LicensePlate(req.Plate));
        await _db.SaveChangesAsync(ct);

        return new MotorcycleResponse(
            m.Id.ToString(),
            m.Year,
            m.Model,
            m.Plate.Value
        );
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var hasRental = await _db.Rentals.AnyAsync(r => r.MotorcycleId == id, ct);
        if (hasRental) return Conflict(new { message = "motorcycle_has_rentals" });

        var m = await _db.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        _db.Motorcycles.Remove(m);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
