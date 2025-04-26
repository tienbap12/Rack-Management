using Rack.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Rack.Contracts.Component.Request;

public record UpdateComponentRequest(
// ID sẽ nằm trong route hoặc một phần khác của Command
string? Name,
string? ComponentType,
ComponentStatus? Status,
Guid? StorageRackId,
Guid? CurrentDeviceId,
string? Manufacturer,
string? Model,
string? SpecificationDetails
);