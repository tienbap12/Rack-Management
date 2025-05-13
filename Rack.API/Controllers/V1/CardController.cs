// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Rack.API.Contracts;
// using Rack.Application.Feature.Card.Commands.Create;
// using Rack.Application.Feature.Card.Commands.Delete;
// using Rack.Application.Feature.Card.Commands.Update;
// using Rack.Application.Feature.Card.Queries.GetAll;
// using Rack.Application.Feature.Card.Queries.GetById;
// using Rack.Contracts.Card.Request;
// using System;
// using System.Threading.Tasks;

// namespace Rack.API.Controllers.V1;

// public class CardController : ApiController
// {
//     [HttpGet]
//     [Route(ApiRoutesV1.Card.GetAll)]
//     public async Task<IActionResult> GetAll([FromQuery] Guid? deviceId)
//     {
//         var query = new GetAllCardQuery(deviceId);
//         var result = await Mediator.Send(query);
//         return ToActionResult(result);
//     }

//     [HttpGet]
//     [Route(ApiRoutesV1.Card.GetById)]
//     public async Task<IActionResult> GetById([FromRoute] Guid cardId)
//     {
//         var query = new GetCardByIdQuery(cardId);
//         var result = await Mediator.Send(query);
//         return ToActionResult(result);
//     }

//     [HttpPost]
//     [Authorize(Roles = "Admin")]
//     [Route(ApiRoutesV1.Card.Create)]
//     public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
//     {
//         var command = new CreateCardCommand(request);
//         var result = await Mediator.Send(command);
//         return ToActionResult(result);
//     }

//     [HttpPatch]
//     [Route(ApiRoutesV1.Card.Update)]
//     public async Task<IActionResult> Update([FromRoute] Guid cardId, [FromBody] UpdateCardRequest request)
//     {
//         var command = new UpdateCardCommand(cardId, request);
//         var result = await Mediator.Send(command);
//         return ToActionResult(result);
//     }

//     [HttpDelete]
//     [Route(ApiRoutesV1.Card.Delete)]
//     public async Task<IActionResult> Delete([FromRoute] Guid cardId)
//     {
//         var command = new DeleteCardCommand(cardId);
//         var result = await Mediator.Send(command);
//         return ToActionResult(result);
//     }
// } 