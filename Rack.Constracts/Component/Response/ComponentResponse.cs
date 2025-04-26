using Rack.Contracts.Audit;
using Rack.Domain.Enum;

namespace Rack.Contracts.Component.Response;

public record ComponentResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string ComponentType { get; init; }
    public ComponentStatus Status { get; init; }
    public string? SerialNumber { get; init; }
    public Guid? StorageRackId { get; init; }
    public Guid? CurrentDeviceId { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SpecificationDetails { get; init; }
}