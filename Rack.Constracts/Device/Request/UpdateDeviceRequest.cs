using Rack.Domain.Enum;

namespace Rack.Contracts.Device.Requests;

public record UpdateDeviceRequest(
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