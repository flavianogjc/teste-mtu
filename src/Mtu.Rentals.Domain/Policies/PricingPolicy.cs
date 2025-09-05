// src/Mtu.Rentals.Domain/Policies/PricingPolicy.cs
using System;

namespace Mtu.Rentals.Domain.Entities;

public static class PricingPolicy
{
    public static decimal GetDailyRate(RentalPlan plan) => plan switch
    {
        RentalPlan.Days7  => 30m,
        RentalPlan.Days15 => 28m,
        RentalPlan.Days30 => 22m,
        RentalPlan.Days45 => 20m,
        RentalPlan.Days50 => 18m,
        _ => throw new ArgumentOutOfRangeException(nameof(plan))
    };

    // Agora DateTime; mantemos “date-only” (00:00:00 UTC), inclusivo
    public static DateTime CalculateScheduledEnd(DateTime start, RentalPlan plan)
        => AsDateUtc(start).AddDays((int)plan - 1);

    // returned também DateTime
    public static RentalCharge CalculateTotal(Rental r, DateTime returned)
    {
        var start    = r.StartDate.Date;      // garante comparação por data
        var expected = r.ExpectedEndDate.Date;
        var retDate  = returned.Date;

        var usedDays = Math.Max(1, (retDate - start).Days + 1);

        var planDays = (int)r.Plan;
        var baseCost = usedDays <= planDays
            ? usedDays * r.DailyRate
            : planDays * r.DailyRate; // diária base só nos dias do plano

        decimal extra = 0;

        if (retDate < expected)
        {
            var unusedDays = planDays - usedDays;
            var percent = r.Plan switch
            {
                RentalPlan.Days7  => 0.20m,
                RentalPlan.Days15 => 0.40m,
                _ => 0m
            };
            extra = Math.Max(0, unusedDays) * r.DailyRate * percent;
        }
        else if (retDate > expected)
        {
            var lateDays = (retDate - expected).Days;
            extra = lateDays * 50m; // multa fixa por dia de atraso
        }

        return new RentalCharge(baseCost + extra, baseCost, extra);
    }

    // --- helper ---
    private static DateTime AsDateUtc(DateTime dt)
    {
        var u = dt.Kind switch
        {
            DateTimeKind.Utc         => dt,
            DateTimeKind.Local       => dt.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
        return new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}

public sealed record RentalCharge(decimal Total, decimal Base, decimal Extra);
