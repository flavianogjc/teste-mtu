namespace Mtu.Rentals.Domain.Entities;

public sealed class Rental
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MotorcycleId { get; private set; }
    public Guid CourierId { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime ExpectedEndDate { get; private set; }
    public RentalPlan Plan { get; private set; }
    public decimal DailyRate { get; private set; }
    public DateTime? ReturnDate { get; private set; }

    private Rental() { }

    public Rental(Guid motorcycleId, Guid courierId, DateTime start, RentalPlan plan)
    {
        MotorcycleId = motorcycleId;
        CourierId = courierId;
        StartDate = start;
        Plan = plan;

        DailyRate = PricingPolicy.GetDailyRate(plan);
        EndDate = PricingPolicy.CalculateScheduledEnd(start, plan);
        ExpectedEndDate = EndDate;
    }

    public RentalCharge Close(DateTime returnDate)
    {
        ReturnDate = returnDate;
        return PricingPolicy.CalculateTotal(this, returnDate);
    }
}
