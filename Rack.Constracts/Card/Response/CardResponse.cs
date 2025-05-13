using Rack.Contracts.Audit;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;

namespace Rack.Contracts.Card.Response;

public record CardResponse : BaseAuditDto
{
    public Guid Id { get; set; }
    public Guid DeviceID { get; set; }
    public string CardType { get; init; }
    public string CardName { get; init; }
    public string? SerialNumber { get; init; }
    public IEnumerable<PortResponse>? Ports { get; init; } = new List<PortResponse>();
}