namespace Rack.Contracts.ConfigurationItem.Requests;

public record CreateConfigurationItemRequest(
    Guid DeviceId,
    string Value,
    string ConfigType
);