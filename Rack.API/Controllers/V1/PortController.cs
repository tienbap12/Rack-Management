using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.Port.Queries.GetAll;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class PortController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.Port.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? deviceId, [FromQuery] Guid? cardId)
    {
        var query = new GetAllPortQuery(deviceId, cardId);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    // [HttpGet]
    // [Route(ApiRoutesV1.Port.GetById)]
    // public async Task<IActionResult> GetById([FromRoute] Guid portId)
    // {
    //     var query = new GetPortByIdQuery(portId);
    //     var result = await Mediator.Send(query);
    //     return ToActionResult(result);
    // }

    // [HttpPost]
    // [Authorize(Roles = "Admin")]
    // [Route(ApiRoutesV1.Port.Create)]
    // public async Task<IActionResult> Create([FromBody] CreatePortRequest request)
    // {
    //     var command = new CreatePortCommand(request);
    //     var result = await Mediator.Send(command);
    //     return ToActionResult(result);
    // }

    // [HttpPatch]
    // [Route(ApiRoutesV1.Port.Update)]
    // public async Task<IActionResult> Update([FromRoute] Guid portId, [FromBody] UpdatePortRequest request)
    // {
    //     var command = new UpdatePortCommand(portId, request);
    //     var result = await Mediator.Send(command);
    //     return ToActionResult(result);
    // }

    // [HttpDelete]
    // [Route(ApiRoutesV1.Port.Delete)]
    // public async Task<IActionResult> Delete([FromRoute] Guid portId)
    // {
    //     var command = new DeletePortCommand(portId);
    //     var result = await Mediator.Send(command);
    //     return ToActionResult(result);
    // }
} 