using Rack.Contracts.Audit;

namespace Rack.Contracts.ConfigurationItem.Response;

public record ConfigurationItemResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string Name { get; init; }
    public string Value { get; init; }
    public string Type { get; init; }
    public string? Description { get; init; }
}