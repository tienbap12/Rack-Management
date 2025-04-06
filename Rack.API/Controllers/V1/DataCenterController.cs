using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.DataCenter.Commands.Create;
using Rack.Application.Feature.DataCenter.Commands.Delete;
using Rack.Application.Feature.DataCenter.Commands.Update;
using Rack.Contracts.DataCenter;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class DataCenterController : ApiController
{
    [HttpPost]
    [Route(ApiRoutesV1.DataCenter.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDataCenterDto request)
    {
        var command = new CreateDataCenterCommand(request);
        return Ok(await Mediator.Send(command));
    }

    [HttpPatch]
    [Route(ApiRoutesV1.DataCenter.Update)]
    public async Task<IActionResult> Update([FromBody] Guid DCId, UpdateDataCenterDto request)
    {
        var command = new UpdateDataCenterCommand(DCId, request);
        return Ok(await Mediator.Send(command));
    }

    [HttpDelete]
    [Route(ApiRoutesV1.DataCenter.Delete)]
    public async Task<IActionResult> Delete(Guid DCId)
    {
        var command = new DeleteDataCenterCommand(DCId);
        return Ok(await Mediator.Send(command));
    }
}