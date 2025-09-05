using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mtu.Rentals.Infrastructure.Persistence;
using Mtu.Rentals.Domain.Entities;

[ApiController]
[Route("locacao")]
public sealed class RentalsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RentalsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<RentalResponse>> Create([FromBody] CreateRentalRequest req, CancellationToken ct)
    {
        var courier = await _db.Couriers.FindAsync(new object?[] { req.courierId }, ct);
        if (courier is null) return BadRequest(new { message = "Dados inválidos" });
        if ((courier.CnhType & CnhType.A) == 0) return BadRequest(new { message = "Dados inválidos", detailing="Not Fount Courier!"});

        var motoExists = await _db.Motorcycles.AnyAsync(m => m.Id == req.motorcycleId, ct);
        if (!motoExists) return BadRequest(new { message = "Dados inválidos", detailing="Not Fount Motorcycle!"});

        var start = DateTime.UtcNow.Date.AddDays(1);
        var plan = (RentalPlan)req.planDays;

        var rental = new Rental(req.motorcycleId, req.courierId, start, plan);
        _db.Rentals.Add(rental);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = rental.Id },
            new RentalResponse(rental.Id, rental.MotorcycleId, rental.CourierId,
                rental.StartDate, rental.EndDate, rental.ExpectedEndDate,
                (int)rental.Plan, rental.DailyRate));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalResponse>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _db.Rentals.FindAsync(new object?[] { id }, ct);
        if (r is null) return NotFound();

        return new RentalResponse(r.Id, r.MotorcycleId, r.CourierId,
            r.StartDate, r.EndDate, r.ExpectedEndDate,
            (int)r.Plan, r.DailyRate);
    }

    [HttpPost("{id:guid}/devolucao")]
    public async Task<ActionResult<ReturnRentalResponse>> Return(Guid id, [FromBody] ReturnRentalRequest req, CancellationToken ct)
    {
        var r = await _db.Rentals.FindAsync(new object?[] { id }, ct);
        if (r is null) return NotFound();

        var charge = r.Close(req.returnDate);
        await _db.SaveChangesAsync(ct);

        return new ReturnRentalResponse(r.Id, charge.Total, charge.Base, charge.Extra);
    }
}
