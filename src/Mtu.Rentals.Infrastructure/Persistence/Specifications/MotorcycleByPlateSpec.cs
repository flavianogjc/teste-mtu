using Ardalis.Specification;
using Mtu.Rentals.Domain.Entities;

public sealed class MotorcycleByPlateSpec : Specification<Motorcycle>
{
    public MotorcycleByPlateSpec(string plate)
    {
        Query.Where(m => m.Plate.Normalized == plate.ToUpper().Replace("-", ""));
    }
}