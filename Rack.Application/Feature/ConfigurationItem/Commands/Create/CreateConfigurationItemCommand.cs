using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Create;

public class CreateConfigurationItemCommand(CreateConfigurationItemRequest createConfigurationItemRequest) : ICommand<Response>
{
    public Guid DeviceId => createConfigurationItemRequest.DeviceId;
    public string Name => createConfigurationItemRequest.Name;
    public string Value => createConfigurationItemRequest.Value;
    public string ConfigType => createConfigurationItemRequest.ConfigType;
    public string? Description => createConfigurationItemRequest.Description;
}