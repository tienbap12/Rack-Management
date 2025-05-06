using Rack.Contracts.Audit;
using Rack.Contracts.Card.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
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
    public IEnumerable<ConfigurationItemResponse>? ConfigurationItems { get; init; } = new List<ConfigurationItemResponse>();
    public IEnumerable<CardResponse>? Cards { get; init; } = new List<CardResponse>();
    public IEnumerable<PortResponse>? Ports { get; init; } = new List<PortResponse>();
    public IEnumerable<DeviceResponse>? ChildDevices { get; init; } = new List<DeviceResponse>();
    public IEnumerable<PortConnectionResponse>? PortConnections { get; init; } = new List<PortConnectionResponse>();
}
