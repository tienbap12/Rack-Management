using Rack.Domain.Enum;

namespace Rack.Contracts.Component.Request;

public record CreateComponentRequest(
string Name,
string ComponentType,
ComponentStatus Status,
string? SerialNumber,
Guid? StorageRackId,
string? Manufacturer,
string? Model,
string? SpecificationDetails
);