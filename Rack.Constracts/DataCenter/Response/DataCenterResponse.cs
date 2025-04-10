using Rack.Contracts.DeviceRack.Response;

namespace Rack.Contracts.DataCenter.Response;

public class DataCenterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public DateTime CreatedDate { get; set; }

    // Optional: If you want to include rack count or details
    public ICollection<DeviceRackResponse> Racks { get; set; } = new List<DeviceRackResponse>();
}