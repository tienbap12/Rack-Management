using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Update;

public class UpdateConfigurationItemCommand(Guid ConfigItemId, UpdateConfigurationItemRequest updateConfigurationItemRequest) : ICommand<Response>
{
    public Guid Id => ConfigItemId;
    public Guid DeviceId => updateConfigurationItemRequest.DeviceId;
    public string ConfigType { get; set; }
    public string ConfigValue { get; set; }
    public string LastModifiedBy { get; set; }
}