using Rack.Contracts.Audit;
using Rack.Contracts.Port.Response;

namespace Rack.Contracts.Card.Response;

public record CardResponse : BaseAuditDto
{
public Guid DeviceID { get; set; }
    public string CardType { get; init; }
    public string CardName { get; init; }
    public string? SerialNumber { get; init; }
    public IEnumerable<PortResponse>? Ports { get; init; } = new List<PortResponse>();
}