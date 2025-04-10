using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.Customer.Commands.Create;
using Rack.Application.Feature.Customer.Commands.Delete;
using Rack.Application.Feature.Customer.Commands.Update;
using Rack.Application.Feature.Customer.Queries.GetAll;
using Rack.Application.Feature.Customer.Queries.GetById;
using Rack.Contracts.Customer.Request;
using System;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1
{
    public class CustomerController : ApiController
    {
        [HttpGet]
        [Route(ApiRoutesV1.Customer.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCustomerQuery();
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(ApiRoutesV1.Customer.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid customerId)
        {
            var query = new GetCustomerByIdQuery(customerId);
            var result = await Mediator.Send(query);
            return ToActionResult(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route(ApiRoutesV1.Customer.Create)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            var command = new CreateCustomerCommand(request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpPatch]
        [Route(ApiRoutesV1.Customer.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid customerId, [FromBody] UpdateCustomerRequest request)
        {
            var command = new UpdateCustomerCommand(customerId, request);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(ApiRoutesV1.Customer.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid customerId)
        {
            var command = new DeleteCustomerCommand(customerId);
            var result = await Mediator.Send(command);
            return ToActionResult(result);
        }
    }
}