using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.Role.Commands.Create;
using Rack.Application.Feature.Role.Commands.Delete;
using Rack.Application.Feature.Role.Commands.Update;
using Rack.Application.Feature.Role.Queries.GetAll;
using Rack.Application.Feature.Role.Queries.GetById;
using Rack.Contracts.Role.Requests;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class RoleController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.Role.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllRolesQuery();
        return ToActionResult(await Mediator.Send(query));
    }

    [HttpGet]
    [Route(ApiRoutesV1.Role.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid roleId)
    {
        var query = new GetRoleByIdQuery(roleId);
        return ToActionResult(await Mediator.Send(query));
    }

    [HttpPost]
    [Route(ApiRoutesV1.Role.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand(request);
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpPatch]
    [Route(ApiRoutesV1.Role.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(roleId, request);
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpDelete]
    [Route(ApiRoutesV1.Role.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid roleId)
    {
        var command = new DeleteRoleCommand(roleId);
        return ToActionResult(await Mediator.Send(command));
    }
}