using Rack.Contracts.Audit;
using Rack.Contracts.Device.Responses;

namespace Rack.Contracts.DeviceRack.Response;

public record DeviceRackResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid DataCenterID { get; init; }
    public string RackNumber { get; init; }
    public int? Size { get; init; }
    public List<DeviceResponse> Devices { get; init; } = new();
}

public record DeviceQuickResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int? Size { get; init; }
    public string DeviceType { get; init; }
    public string Status { get; init; }
    public int? UPosition { get; init; }
    public string IpAddress { get; init; }
    // Thêm trường cần thiết khác nếu UI cần
}

public record DeviceRackQuickResponse
{
    public Guid Id { get; init; }
    public string RackNumber { get; init; }
    public int? Size { get; init; }
    public string DataCenterName { get; init; }
    public string DataCenterLocation { get; init; }
    public List<DeviceQuickResponse> Devices { get; init; } = new();
}