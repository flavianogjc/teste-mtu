namespace Mtu.Rentals.Domain.Entities;

public sealed record Cnpj
{
    public string Digits { get; }

    public Cnpj(string value)
    {
        Digits = new string(value.Where(char.IsDigit).ToArray());
        if (Digits.Length != 14)
            throw new ArgumentException("invalid_cnpj", nameof(value));
    }

    private Cnpj() { }
}
