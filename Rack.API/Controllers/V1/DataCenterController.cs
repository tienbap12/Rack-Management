using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.DataCenter.Commands.Create;
using Rack.Application.Feature.DataCenter.Commands.Delete;
using Rack.Application.Feature.DataCenter.Commands.Update;
using Rack.Application.Feature.DataCenter.Queries.GetAll;
using Rack.Application.Feature.DataCenter.Queries.GetById;
using Rack.Contracts.DataCenter.Requests;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class DataCenterController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.DataCenter.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllDataCenterQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutesV1.DataCenter.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid dcId)
    {
        var query = new GetDataCenterByIdQuery(dcId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutesV1.DataCenter.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDataCenterRequest request)
    {
        var command = new CreateDataCenterCommand(request);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch]
    [Route(ApiRoutesV1.DataCenter.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid dcId, [FromBody] UpdateDataCenterRequest request)
    {
        var command = new UpdateDataCenterCommand(dcId, request);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete]
    [Route(ApiRoutesV1.DataCenter.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid dcId)
    {
        var command = new DeleteDataCenterCommand(dcId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}