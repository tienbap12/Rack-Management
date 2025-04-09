namespace Rack.Contracts.DeviceRack.Requests;

public record UpdateDeviceRackRequest(
    Guid DataCenterID,
    string RackNumber,
    string? Size
);