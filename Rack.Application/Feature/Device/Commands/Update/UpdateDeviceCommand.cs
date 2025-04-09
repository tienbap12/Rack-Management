using Rack.Contracts.Device.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Device.Commands.Update;

public class UpdateDeviceCommand(Guid deviceId, UpdateDeviceRequest updateDeviceRequest) : ICommand<Response>
{
    public Guid DeviceId => deviceId;
    public Guid? ParentDeviceID => updateDeviceRequest.ParentDeviceID;
    public Guid? RackID => updateDeviceRequest.RackID;
    public string? PositionInRack => updateDeviceRequest.PositionInRack;
    public string? SlotInParent => updateDeviceRequest.SlotInParent;
    public string DeviceType => updateDeviceRequest.DeviceType;
    public string? Manufacturer => updateDeviceRequest.Manufacturer;
    public string? SerialNumber => updateDeviceRequest.SerialNumber;
    public string? Model => updateDeviceRequest.Model;
    public string Status => updateDeviceRequest.Status;
}