namespace Rack.Contracts.ConfigurationItem.Response;

public record ConfigurationItemResponse(
    Guid Id,
    Guid DeviceId,
    string Name,
    string Value,
    string Type,
    string? Description,
    DateTime CreatedOn,
    string CreatedBy,
    DateTime? LastModifiedOn,
    string? LastModifiedBy
); 