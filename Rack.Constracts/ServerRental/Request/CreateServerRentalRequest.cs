namespace Rack.Contracts.ServerRental.Request;

public record CreateServerRentalRequest(
    Guid CustomerID,
    Guid DeviceID,
    DateTime StartDate,
    DateTime? EndDate
);