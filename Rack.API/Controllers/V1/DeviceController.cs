using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.Device.Commands.Create;
using Rack.Application.Feature.Device.Commands.Delete;
using Rack.Application.Feature.Device.Commands.Update;
using Rack.Application.Feature.Device.Queries.GetAll;
using Rack.Application.Feature.Device.Queries.GetById;
using Rack.Contracts.Device.Requests;
using Rack.Domain.Enum;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class DeviceController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.Device.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? rackId, [FromQuery] DeviceStatus? status)
    {
        var query = new GetAllDeviceQuery(rackId, status);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    [HttpGet]
    [Route(ApiRoutesV1.Device.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid deviceId)
    {
        var query = new GetDeviceByIdQuery(deviceId);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(ApiRoutesV1.Device.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request)
    {
        var command = new CreateDeviceCommand(request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpPatch]
    [Route(ApiRoutesV1.Device.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid deviceId, [FromBody] UpdateDeviceRequest request)
    {
        var command = new UpdateDeviceCommand(deviceId, request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpDelete]
    [Route(ApiRoutesV1.Device.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid deviceId)
    {
        var command = new DeleteDeviceCommand(deviceId);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }
}