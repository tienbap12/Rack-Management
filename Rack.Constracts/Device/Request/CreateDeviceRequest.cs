using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Domain.Enum;

namespace Rack.Contracts.Device.Requests;

public record CreateDeviceRequest(
    // --- ID Quan hệ ---
    Guid? ParentDeviceID,
    Guid? RackID,

    // --- Thuộc tính Vật lý & Vị trí ---
    int Size,
    string Name,
    int? UPosition,
    string? SlotInParent,

    // --- Thông tin Cơ bản & Phân loại ---
    string DeviceType,
    DeviceStatus Status,
    string? IpAddress,
    string? Manufacturer,
    string? SerialNumber,
    string? Model,
    List<CreateConfigurationItemRequest>? ConfigurationItems
);