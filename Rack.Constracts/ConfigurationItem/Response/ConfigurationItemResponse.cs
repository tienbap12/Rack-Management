using Rack.Contracts.Audit;

namespace Rack.Contracts.ConfigurationItem.Response;

public record ConfigurationItemResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string ConfigValue { get; init; }
    public string ConfigType { get; init; }
}