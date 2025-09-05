public sealed record CreateRentalRequest(Guid motorcycleId, Guid courierId, int planDays);

public sealed record RentalResponse(
    Guid id,
    Guid motorcycleId,
    Guid courierId,
    DateTime startDate,
    DateTime endDate,
    DateTime expectedEndDate,
    int planDays,
    decimal dailyRate);

public sealed record ReturnRentalRequest(DateTime returnDate);

public sealed record ReturnRentalResponse(Guid rentalId, decimal total, decimal baseValue, decimal extra);
