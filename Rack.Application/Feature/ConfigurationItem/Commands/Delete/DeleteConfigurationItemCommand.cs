using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Delete;

public class DeleteConfigurationItemCommand(Guid configItemId) : ICommand<Response>
{
    public Guid Id { get; set; } = configItemId;
}