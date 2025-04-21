using Rack.Contracts.Role.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Role.Commands.Update;

public class UpdateRoleCommand(Guid roleId, UpdateRoleRequest _request) : ICommand<Response>
{
    public Guid Id => roleId;
    public string Name => _request.Name;
    public string Status => _request.Status;
}