using Rack.Domain.Enum;

namespace Rack.Contracts.Device.Requests;

public record CreateDeviceRequest(
    Guid? ParentDeviceID,
    Guid? RackID,
    int Size,
    string Name,
    string IpAddress,
    string? PositionInRack,
    string? SlotInParent,
    string DeviceType,
    string? Manufacturer,
    string? SerialNumber,
    string? Model,
    string Status = StatusDevice.ACTIVE
);