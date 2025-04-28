using Rack.Contracts.Audit;
using Rack.Contracts.Card.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Port.Response;
using Rack.Domain.Enum;

namespace Rack.Contracts.Device.Responses;

public record DeviceResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid? ParentDeviceID { get; init; }
    public Guid? RackID { get; init; }
    public int Size { get; init; }
    public string Name { get; init; }
    public string IpAddress { get; init; }
    public int? UPosition { get; init; }
    public string? SlotInParent { get; init; }
    public string DeviceType { get; init; }
    public string? Manufacturer { get; init; }
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
    public DeviceStatus Status { get; init; }
    IEnumerable<ConfigurationItemResponse>? ConfigurationItems { get; init; } = new List<ConfigurationItemResponse>();
    IEnumerable<CardResponse>? Cards { get; init; } = new List<CardResponse>();
    IEnumerable<PortResponse>? Ports { get; init; } = new List<PortResponse>();
}