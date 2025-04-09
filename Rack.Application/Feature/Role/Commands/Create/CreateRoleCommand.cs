using Rack.Contracts.Role.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Role.Commands.Create;

public class CreateRoleCommand(CreateRoleRequest request) : ICommand<Response>
{
    public string Name => request.Name;
    public string Status => request.Status;
}