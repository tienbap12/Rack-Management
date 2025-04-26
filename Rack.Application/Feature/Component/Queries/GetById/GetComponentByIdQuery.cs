using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Component.Queries.GetById;

public class GetComponentByIdQuery(Guid Id) : IQuery<ComponentResponse>
{
    public Guid Id { get; } = Id;
}