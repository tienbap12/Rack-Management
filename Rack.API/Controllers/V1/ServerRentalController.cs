using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.ServerRental.Commands.Create;
using Rack.Application.Feature.ServerRental.Commands.Delete;
using Rack.Application.Feature.ServerRental.Commands.Update;
using Rack.Application.Feature.ServerRental.Queries.GetAll;
using Rack.Application.Feature.ServerRental.Queries.GetById;
using Rack.Contracts.ServerRental.Request;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1
{
    public class ServerRentalController : ApiController
    {
        [HttpGet]
        [Route(ApiRoutesV1.ServerRental.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllServerRentalQuery();
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(ApiRoutesV1.ServerRental.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid rentalId)
        {
            var query = new GetServerRentalByIdQuery(rentalId);
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route(ApiRoutesV1.ServerRental.Create)]
        public async Task<IActionResult> Create([FromBody] CreateServerRentalRequest request)
        {
            var command = new CreateServerRentalCommand(request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpPatch]
        [Route(ApiRoutesV1.ServerRental.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid rentalId, [FromBody] UpdateServerRentalRequest request)
        {
            var command = new UpdateServerRentalCommand(rentalId, request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(ApiRoutesV1.ServerRental.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid rentalId)
        {
            var command = new DeleteServerRentalCommand(rentalId);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }
    }
}