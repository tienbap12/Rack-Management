using Rack.Contracts.Authentication;
using Rack.Contracts.Authentication.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.RefreshToken;

public class RefreshTokenCommand(RefreshTokenRequest refreshToken) : ICommand<Response<TokenResponse>>
{
    public string RefreshToken => refreshToken.RefreshToken;
}