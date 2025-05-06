using Rack.Contracts.Audit;
using Rack.Contracts.PortConnection.Response;

namespace Rack.Contracts.Port.Response
{
    public record PortResponse : BaseAuditDto
    {
    public Guid DeviceID { get; init; }
    public Guid? CardID { get; init; }
    public string PortName { get; init; } = null!;
    public string PortType { get; init; } = null!;
    public IEnumerable<PortConnectionResponse>? Connections { get; init; } = new List<PortConnectionResponse>();
    }
}