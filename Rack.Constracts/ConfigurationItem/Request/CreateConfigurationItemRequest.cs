namespace Rack.Contracts.ConfigurationItem.Requests;

public record CreateConfigurationItemRequest(
    Guid DeviceId,
    string Name,
    string Value,
    string ConfigType,
    string? Description = null
);