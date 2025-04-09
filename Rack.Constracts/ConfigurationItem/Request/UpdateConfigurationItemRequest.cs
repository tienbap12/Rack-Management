namespace Rack.Contracts.ConfigurationItem.Requests;

public record UpdateConfigurationItemRequest(
    Guid DeviceId,
    string Name,
    string Value,
    string ConfigType,
    string? Description = null
);