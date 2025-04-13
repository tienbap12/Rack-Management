using Microsoft.AspNetCore.Mvc;
using Rack.Application.Commons.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class ZabbixController : ControllerBase
{
    private readonly IZabbixService _zabbixService;

    public ZabbixController(IZabbixService zabbixService)
    {
        _zabbixService = zabbixService;
    }

    // GET: api/zabbix/hosts
    [HttpGet("hosts")]
    public async Task<ActionResult<IEnumerable<ZabbixHostSummary>>> GetMonitoredHosts(CancellationToken cancellationToken)
    {
        var hosts = await _zabbixService.GetMonitoredHostsAsync(cancellationToken);
        return Ok(hosts);
    }

    // GET: api/zabbix/hosts/{hostId}
    [HttpGet("hosts/{hostId}")]
    public async Task<ActionResult<ZabbixHostDetail>> GetHostDetail(string hostId, CancellationToken cancellationToken)
    {
        var hostDetail = await _zabbixService.GetHostDetailsAsync(hostId, cancellationToken);
        if (hostDetail == null)
        {
            return NotFound(new { Message = $"Host with id '{hostId}' not found" });
        }
        return Ok(hostDetail);
    }

    // GET: api/zabbix/hosts/{hostId}/problems
    [HttpGet("hosts/{hostId}/problems")]
    public async Task<ActionResult<IEnumerable<ZabbixProblemInfo>>> GetProblemsByHost(string hostId, CancellationToken cancellationToken)
    {
        var problems = await _zabbixService.GetProblemsByHostAsync(hostId, cancellationToken);
        return Ok(problems);
    }

    // GET: api/zabbix/problems/recent
    // Query string example: ?severityThreshold=2&limit=50
    [HttpGet("problems/recent")]
    public async Task<ActionResult<IEnumerable<ZabbixProblemInfo>>> GetRecentProblems(
        [FromQuery] int severityThreshold = 0,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var problems = await _zabbixService.GetRecentProblemsAsync(severityThreshold, limit, cancellationToken);
        return Ok(problems);
    }

    // GET: api/zabbix/hosts/{hostId}/resources
    // Query example: api/zabbix/hosts/10101/resources?keys=system.cpu.load,system.mem.free
    [HttpGet("hosts/{hostId}/resources")]
    public async Task<ActionResult<Dictionary<string, string>>> GetHostResourceItems(
        string hostId,
        [FromQuery(Name = "keys")] IEnumerable<string> itemKeys,
        CancellationToken cancellationToken)
    {
        var resources = await _zabbixService.GetHostResourceItemsAsync(hostId, itemKeys, cancellationToken);
        return Ok(resources);
    }
}