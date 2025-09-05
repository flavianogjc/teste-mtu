namespace Mtu.Rentals.Contracts;

public interface IMotorcycleRegistered
{
    Guid MotorcycleId { get; }
    int Year { get; }
    string Model { get; }
}