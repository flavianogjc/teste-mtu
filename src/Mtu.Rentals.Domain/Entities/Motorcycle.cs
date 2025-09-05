namespace Mtu.Rentals.Domain.Entities;

public sealed class Motorcycle
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Identifier { get; private set; }
    public int Year { get; private set; }
    public string Model { get; private set; }
    public LicensePlate Plate { get; private set; }

    private Motorcycle() { }

    public Motorcycle(string identifier, int year, string model, LicensePlate plate)
    {
        Identifier = identifier.Trim();
        Year = year;
        Model = model.Trim();
        Plate = plate;
    }

    public void UpdatePlate(LicensePlate plate) => Plate = plate;
}