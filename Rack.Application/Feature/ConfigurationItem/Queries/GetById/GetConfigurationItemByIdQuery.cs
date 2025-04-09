using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetById;

public class GetConfigurationItemByIdQuery(Guid configItemId) : IQuery<ConfigurationItemResponse>
{
    public Guid Id { get; set; } = configItemId;
}