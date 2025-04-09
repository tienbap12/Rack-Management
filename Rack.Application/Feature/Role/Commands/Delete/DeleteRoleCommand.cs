using Rack.Contracts.Role.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Role.Commands.Delete;

public class DeleteRoleCommand(Guid roleId) : ICommand<Response>
{
    public Guid Id => roleId;
}