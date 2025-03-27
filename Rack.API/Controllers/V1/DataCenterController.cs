using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using Rack.Application.Feature.DataCenter.Commands;
using Rack.Contracts.DataCenter;
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
}
