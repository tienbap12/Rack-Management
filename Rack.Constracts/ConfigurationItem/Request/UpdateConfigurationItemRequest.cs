namespace Rack.Contracts.ConfigurationItem.Requests;

public record UpdateConfigurationItemRequest(
    Guid DeviceId,
    string? ConfigValue,
    string? ConfigType
);