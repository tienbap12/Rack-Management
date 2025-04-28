using Rack.Contracts.Audit;

namespace Rack.Contracts.Card.Response;

public record CardResponse : BaseAuditDto
{
    public Guid DeviceID { get; set; }
    public string CardType { get; init; }
    public string CardName { get; init; }
    public string? SerialNumber { get; init; }
}