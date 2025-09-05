using MassTransit;
using Mtu.Rentals.Contracts;
using Mtu.Rentals.Infrastructure.Persistence;

public sealed class MotorcycleRegisteredConsumer : IConsumer<IMotorcycleRegistered>
{
    private readonly AppDbContext _db;
    public MotorcycleRegisteredConsumer(AppDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<IMotorcycleRegistered> context)
    {
        if (context.Message.Year == 2024)
        {
            _db.MotorcycleNotifications.Add(new MotorcycleNotification
            {
                MotorcycleId = context.Message.MotorcycleId,
                Year = 2024
            });
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}