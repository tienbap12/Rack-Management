using Rack.Contracts.Device.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Device.Commands.Create;

public class CreateDeviceCommand(CreateDeviceRequest request) : ICommand<Response>
{
    public Guid? ParentDeviceID => request.ParentDeviceID;
    public Guid? RackID => request.RackID;
    public int Size => request.Size;
    public string Name => request.Name;
    public string IpAddress => request.IpAddress;
    public string? PositionInRack => request.PositionInRack;
    public string? SlotInParent => request.SlotInParent;
    public string DeviceType => request.DeviceType;
    public string? Manufacturer => request.Manufacturer;
    public string? SerialNumber => request.SerialNumber;
    public string? Model => request.Model;
    public string Status => request.Status;
}