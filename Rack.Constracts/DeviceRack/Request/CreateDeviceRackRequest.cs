namespace Rack.Contracts.DeviceRack.Requests;

public record CreateDeviceRackRequest(
    Guid DataCenterID,
    string RackNumber,
    int? Size
);