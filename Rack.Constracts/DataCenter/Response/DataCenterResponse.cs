using Rack.Contracts.Audit;
using Rack.Contracts.DeviceRack.Response;

namespace Rack.Contracts.DataCenter.Response;

public record DataCenterResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Location { get; init; }
    public DateTime CreatedDate { get; init; }
    public ICollection<DeviceRackResponse> Racks { get; init; } = new List<DeviceRackResponse>();
}