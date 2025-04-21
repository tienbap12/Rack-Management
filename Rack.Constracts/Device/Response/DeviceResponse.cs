using Rack.Contracts.Audit;

namespace Rack.Contracts.Device.Responses;

public record DeviceResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid? ParentDeviceID { get; init; }
    public Guid? RackID { get; init; }
    public int Size { get; init; }
    public string Name { get; init; }
    public string IpAddress { get; init; }
    public string? PositionInRack { get; init; }
    public string? SlotInParent { get; init; }
    public string DeviceType { get; init; }
    public string? Manufacturer { get; init; }
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
    public string Status { get; init; }
}