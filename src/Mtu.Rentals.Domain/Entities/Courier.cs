// src/Mtu.Rentals.Domain/Entities/Courier.cs
using System;
using System.Linq;

namespace Mtu.Rentals.Domain.Entities;

public sealed class Courier
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public Cnpj Cnpj { get; private set; }

    public DateTime BirthDate { get; private set; }

    public string CnhNumber { get; private set; }
    public CnhType CnhType { get; private set; }
    public string? CnhImagePath { get; private set; }

    private Courier() { }

    public Courier(string name, Cnpj cnpj, DateTime birthDate, string cnhNumber, CnhType cnhType)
    {
        Name = name.Trim();
        Cnpj = cnpj;
        BirthDate = AsDateUtc(birthDate); // normalizes to 00:00:00 UTC
        CnhNumber = Normalize(cnhNumber);
        CnhType = cnhType;
    }

    public void SetCnhImage(string path) => CnhImagePath = path;

    private static string Normalize(string v)
        => new string(v.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    // Guarantees UTC without “assuming” the wrong time zone
    private static DateTime AsUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc         => dt,
        DateTimeKind.Local       => dt.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc) // treats as UTC
    };

    // Maintains “date-only” semantics: midnight UTC
    private static DateTime AsDateUtc(DateTime dt)
    {
        var u = AsUtc(dt);
        return new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}
