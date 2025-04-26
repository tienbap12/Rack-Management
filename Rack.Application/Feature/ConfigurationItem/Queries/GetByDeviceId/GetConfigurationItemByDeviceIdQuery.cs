using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetById;

public class GetConfigurationItemByDeviceIdQuery(Guid deviceId) : IQuery<List<ConfigurationItemResponse>>
{
    public Guid Id { get; set; } = deviceId;
}