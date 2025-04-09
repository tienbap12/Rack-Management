using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.Login;

public class LoginCommand(LoginRequest request) : ICommand<Response<AuthResponse>>
{
    public string Email { get; set; } = request.Email;
    public string Password { get; set; } = request.Password;
}