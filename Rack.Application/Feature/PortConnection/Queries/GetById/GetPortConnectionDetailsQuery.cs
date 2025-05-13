using Rack.Contracts.PortConnection.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.PortConnection.Queries.GetById;

public record GetPortConnectionDetailsQuery(Guid portConnectionId) : IQuery<PortConnectionResponse>;
