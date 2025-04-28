using Rack.Contracts.Card.Request;
using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Contracts.Port.Request;
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
    DeviceStatus? Status,
    List<CreateCardRequest>? Cards,
    List<CreatePortRequest>? Ports,
    List<CreateConfigurationItemRequest>? ConfigurationItems,
    List<CreateChildDeviceRequest>? ChildDevices
);