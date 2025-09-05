namespace Mtu.Rentals.Domain.Entities;

public sealed class Motorcycle
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int Year { get; private set; }
    public string Model { get; private set; }
    public LicensePlate Plate { get; private set; } = null!;

    private Motorcycle() { }

    public Motorcycle(int year, string model, LicensePlate plate)
    {
        Year = year;
        Model = model.Trim();
        Plate = plate;
    }

    public void UpdatePlate(LicensePlate plate) => Plate = plate;
}