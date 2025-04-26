using Rack.Contracts.Role.Requests;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Role.Commands.Create;

public class CreateRoleCommand(CreateRoleRequest request) : ICommand<Response>
{
    public string Name => request.Name;
    public CommonStatus Status => request.Status;
}