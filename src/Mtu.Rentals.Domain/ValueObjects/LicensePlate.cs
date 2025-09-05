using System.Text.RegularExpressions;

namespace Mtu.Rentals.Domain.Entities;

public sealed record LicensePlate
{
    private static readonly Regex Br = new("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.Compiled);
    public string Value { get; }
    public string Normalized { get; }

    public LicensePlate(string value)
    {
        Value = value.Trim().ToUpperInvariant().Replace("-", string.Empty);
        Normalized = Value;
        if (!Br.IsMatch(Normalized)) throw new ArgumentException("invalid_license_plate", nameof(value));
    }
    public override string ToString() => Value;
}