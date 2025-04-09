using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.ConfigurationItem.Commands.Create;
using Rack.Application.Feature.ConfigurationItem.Commands.Delete;
using Rack.Application.Feature.ConfigurationItem.Commands.Update;
using Rack.Application.Feature.ConfigurationItem.Queries.GetAll;
using Rack.Application.Feature.ConfigurationItem.Queries.GetById;
using Rack.Contracts.ConfigurationItem.Requests;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

public class ConfigurationItemController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.ConfigurationItem.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllConfigurationItemQuery();
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    [HttpGet]
    [Route(ApiRoutesV1.ConfigurationItem.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid configItemId)
    {
        var query = new GetConfigurationItemByIdQuery(configItemId);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    [HttpPost]
    [Route(ApiRoutesV1.ConfigurationItem.Create)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationItemRequest request)
    {
        var command = new CreateConfigurationItemCommand(request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpPatch]
    [Route(ApiRoutesV1.ConfigurationItem.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid configItemId, [FromBody] UpdateConfigurationItemRequest request)
    {
        var command = new UpdateConfigurationItemCommand(configItemId, request);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }

    [HttpDelete]
    [Route(ApiRoutesV1.ConfigurationItem.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid configItemId)
    {
        var command = new DeleteConfigurationItemCommand(configItemId);
        var result = await Mediator.Send(command);
        return ToActionResult(result);
    }
}