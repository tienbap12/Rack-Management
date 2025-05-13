using Rack.Contracts.Card.Request;
using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Contracts.Device.Requests;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.Port.Request;
using Rack.Contracts.PortConnection.Request;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Create;

public class CreateDeviceCommand(CreateDeviceRequest request) : ICommand<Response<DeviceResponse>>
{
    public Guid? ParentDeviceID => request.ParentDeviceID;
    public Guid? RackID => request.RackID;
    public int Size => request.Size;
    public string Name => request.Name;
    public string IpAddress => request.IpAddress;
    public int? UPosition => request.UPosition;
    public string? SlotInParent => request.SlotInParent;
    public string DeviceType => request.DeviceType;
    public string? Manufacturer => request.Manufacturer;
    public string? SerialNumber => request.SerialNumber;
    public string? LinkIdPage => request.LinkIdPage;
    public string? Model => request.Model;
    public DeviceStatus Status => request.Status;
    public List<CreateConfigurationItemRequest>? ConfigurationItems => request.ConfigurationItems;
    public List<CreateCardRequest>? Cards => request.Cards;
    public List<CreatePortRequest>? Ports => request.Ports;
    public List<CreatePortConnectionRequest>? PortConnections => request.PortConnections;
    public List<CreateChildDeviceRequest>? ChildDevices => request.ChildDevices;
}