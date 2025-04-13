namespace Rack.Contracts.ServerRental.Response;

public record ServerRentalResponse(
    Guid Id,
    Guid CustomerID,
    Guid DeviceID,
    string CustomerName,
    string DeviceName,
    DateTime StartDate,
    DateTime? EndDate
);