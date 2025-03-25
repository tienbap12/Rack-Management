using Rack.Application.Wrappers;
using Rack.Contracts.Authentication;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.Register;

public class RegisterCommand(AuthRequest request) : ICommand<Response>
{
    public AuthRequest Request { get; set; } = request;
}