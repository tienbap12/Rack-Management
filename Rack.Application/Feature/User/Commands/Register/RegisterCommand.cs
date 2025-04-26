using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.Register;

public class RegisterCommand(AuthRequest request) : ICommand<Response<AuthResponse>>
{
    public AuthRequest Request { get; set; } = request;
}