namespace Rack.Contracts.ServerRental.Request;

public record UpdateServerRentalRequest(
    Guid CustomerID,
    Guid DeviceID,
    DateTime StartDate,
    DateTime? EndDate
);