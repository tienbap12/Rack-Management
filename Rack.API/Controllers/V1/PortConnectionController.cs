using Microsoft.AspNetCore.Mvc;
using Rack.Application.Feature.PortConnection.Queries;
using Rack.API.Controllers.V1;
using Rack.API.Contracts;
using System;
using System.Threading.Tasks;
using Rack.Application.Feature.PortConnection.Queries.GetById;

namespace Rack.API.Controllers;

public class PortConnectionController : ApiController
{
    [HttpGet]
    [Route(ApiRoutesV1.PortConnection.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid portConnectionId)
    {
        var query = new GetPortConnectionDetailsQuery(portConnectionId);
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }
}