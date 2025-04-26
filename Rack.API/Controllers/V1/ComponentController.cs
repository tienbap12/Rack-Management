using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.Component.Commands.Create;
using Rack.Application.Feature.Component.Commands.Delete;
using Rack.Application.Feature.Component.Commands.Update;
using Rack.Application.Feature.Component.Queries.GetAll;
using Rack.Application.Feature.Component.Queries.GetById;
using Rack.Contracts.Component.Request;
using Rack.Domain.Enum;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1
{
    public class ComponentController : ApiController
    {
        [HttpGet]
        [Route(ApiRoutesV1.Component.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] ComponentStatus? status, [FromQuery] Guid? storageRackId, [FromQuery] Guid? currentDeviceId)
        {
            var query = new GetAllComponentQuery(status, storageRackId, currentDeviceId);
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(ApiRoutesV1.Component.GetById)]
        public async Task<IActionResult> GetById(Guid componentId)
        {
            var query = new GetComponentByIdQuery(componentId);
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpPost]
        [Route(ApiRoutesV1.Component.Create)]
        public async Task<IActionResult> Create([FromBody] CreateComponentRequest request)
        {
            var command = new CreateComponentCommand(request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(ApiRoutesV1.Component.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid componentId, [FromBody] UpdateComponentRequest request)
        {
            var command = new UpdateComponentCommand(componentId, request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(ApiRoutesV1.Component.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid componentId)
        {
            var command = new DeleteComponentCommand(componentId);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }
    }
}