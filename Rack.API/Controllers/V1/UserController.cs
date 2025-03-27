using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.User.Commands.Login;
using Rack.Application.Feature.User.Commands.Register;
using Rack.Contracts.Authentication;
using System.Threading.Tasks;


namespace Rack.API.Controllers.V1;

public class UserController : ApiController
{
    [HttpPost]
    [Route(ApiRoutesV1.Account.Login)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    [Route(ApiRoutesV1.Account.Register)]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        var command = new RegisterCommand(request);
        return Ok(await Mediator.Send(command));
    }
}