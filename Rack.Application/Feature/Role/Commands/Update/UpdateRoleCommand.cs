using Rack.Contracts.Role.Requests;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Role.Commands.Update;

public class UpdateRoleCommand(Guid roleId, UpdateRoleRequest _request) : ICommand<Response>
{
    public Guid Id => roleId;
    public string Name => _request.Name;
    public CommonStatus Status => _request.Status;
}