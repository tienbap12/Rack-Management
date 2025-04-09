namespace Rack.Contracts.Device.Responses;

public record DeviceResponse(
    Guid Id,
    Guid? ParentDeviceID,
    Guid? RackID,
    string? PositionInRack,
    string? SlotInParent,
    string DeviceType,
    string? Manufacturer,
    string? SerialNumber,
    string? Model,
    string Status,
    DateTime CreatedDate,
    DateTime CreatedOn,
    string CreatedBy,
    DateTime? LastModifiedOn,
    string LastModifiedBy
); 