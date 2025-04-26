namespace Rack.Contracts.ConfigurationItem.Requests;

public record CreateConfigurationItemRequest(
    Guid DeviceId,
    string ConfigValue,
    string ConfigType
);