using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rack.API.Contracts;
using Rack.Application.Feature.DeviceRack.Commands.Create;
using Rack.Application.Feature.DeviceRack.Commands.Delete;
using Rack.Application.Feature.DeviceRack.Commands.Update;
using Rack.Application.Feature.DeviceRack.Queries.GetAll;
using Rack.Contracts.DeviceRack.Requests;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class RackController : ApiController
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RackController> _logger;

    public RackController(IMemoryCache cache, ILogger<RackController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    [Route(ApiRoutesV1.Rack.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllDeviceRackQueryOptimized();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutesV1.Rack.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid rackId)
    {
        var query = new GetDeviceRackByIdQuery(rackId);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    [HttpPost]
    [Route(ApiRoutesV1.Rack.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRackRequest request)
    {
        var command = new CreateDeviceRackCommand(request);
        var result = await Mediator.Send(command);

        ClearCache();

        return ToActionResult(result);
    }

    [HttpPatch]
    [Route(ApiRoutesV1.Rack.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid rackId, [FromBody] UpdateDeviceRackRequest request)
    {
        var command = new UpdateDeviceRackCommand(rackId, request);
        var result = await Mediator.Send(command);

        ClearCache();

        return ToActionResult(result);
    }

    [HttpDelete]
    [Route(ApiRoutesV1.Rack.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid rackId)
    {
        var command = new DeleteDeviceRackCommand(rackId);
        var result = await Mediator.Send(command);

        ClearCache();

        return ToActionResult(result);
    }

    private void ClearCache()
    {
        _logger.LogInformation("Clearing device racks cache");
        _cache.Remove("device_racks_all");
    }
}