namespace Rack.Contracts.Device.Requests;

public record UpdateDeviceRequest(
    Guid Id,
    Guid? ParentDeviceID,
    Guid? RackID,
    string? PositionInRack,
    string? SlotInParent,
    string DeviceType,
    string? Manufacturer,
    string? SerialNumber,
    string? Model,
    string Status
); 