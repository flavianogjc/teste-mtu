using System;
using System.Linq;

namespace Mtu.Rentals.Domain.Entities;

public sealed record Cnpj
{
    public string Digits { get; }

    public Cnpj(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("cnpj_required", nameof(value));

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 14)
            throw new ArgumentException("invalid_cnpj", nameof(value));

        Digits = digits;
    }

    private Cnpj() => Digits = string.Empty;

    public override string ToString() => Digits;
}
