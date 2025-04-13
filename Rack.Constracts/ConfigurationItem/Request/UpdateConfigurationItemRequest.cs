namespace Rack.Contracts.ConfigurationItem.Requests;

public record UpdateConfigurationItemRequest(
    Guid DeviceId,
    string Value,
    string ConfigType
);