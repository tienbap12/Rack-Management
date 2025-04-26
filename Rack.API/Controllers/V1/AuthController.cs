using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.User.Commands.Login;
using Rack.Application.Feature.User.Commands.RefreshToken;
using Rack.Application.Feature.User.Commands.Register;
using Rack.Contracts.Authentication;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class AuthController : ApiController
{
    [HttpPost]
    [Route(ApiRoutesV1.Auth.Login)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpPost]
    [Route(ApiRoutesV1.Auth.Register)]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        var command = new RegisterCommand(request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpPost]
    [Route(ApiRoutesV1.Auth.RefreshToken)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }
}