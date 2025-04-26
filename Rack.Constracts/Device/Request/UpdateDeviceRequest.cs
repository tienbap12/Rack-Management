using Rack.Domain.Enum;

namespace Rack.Contracts.Device.Requests;

public record UpdateDeviceRequest(
    Guid? ParentDeviceID,
    Guid? RackID,
    int? Size,
    string? Name,
    int? UPosition,
    string? SlotInParent,
    string? DeviceType,
    string? IpAddress,
    string? Manufacturer,
    string? SerialNumber,
    string? Model,
    DeviceStatus? Status
);