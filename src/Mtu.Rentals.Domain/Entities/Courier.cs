using System;
using System.Linq;

namespace Mtu.Rentals.Domain.Entities;

public sealed class Courier
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Identifier { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Cnpj Cnpj { get; private set; } = null!;

    public DateTime BirthDate { get; private set; }

    public string CnhNumber { get; private set; } = null!;
    public CnhType CnhType { get; private set; }
    public string? CnhImagePath { get; private set; }

    private Courier() { }

    public Courier(string identifier, string name, Cnpj cnpj, DateTime birthDate, string cnhNumber, CnhType cnhType)
    {
        Identifier = identifier.Trim();
        Name = name.Trim();
        Cnpj = cnpj;
        BirthDate = AsDateUtc(birthDate); // normalizes to 00:00:00 UTC
        CnhNumber = cnhNumber;
        CnhType = cnhType;
    }

    public void SetCnhImage(string path) => CnhImagePath = path;

    // Guarantees UTC without “assuming” the wrong time zone
    private static DateTime AsUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc         => dt,
        DateTimeKind.Local       => dt.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
        _ => throw new ArgumentOutOfRangeException(nameof(dt), $"Invalid DateTimeKind: {(int)dt.Kind}")
    };

    // Maintains “date-only” semantics: midnight UTC
    private static DateTime AsDateUtc(DateTime dt)
    {
        var u = AsUtc(dt);
        return new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}
