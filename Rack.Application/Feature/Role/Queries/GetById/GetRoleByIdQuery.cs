using MediatR;
using Rack.Contracts.Role.Responses;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Role.Queries.GetById;

public class GetRoleByIdQuery(Guid roleId) : IQuery<RoleResponse>
{
    public Guid Id { get; set; } = roleId;
}