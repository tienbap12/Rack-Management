using Rack.Contracts.Device.Requests;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Update;

public class UpdateDeviceCommand(Guid deviceId, UpdateDeviceRequest updateDeviceRequest)
    : ICommand<Response<DeviceResponse>>
{
    public Guid DeviceId => deviceId;
    public Guid? ParentDeviceID => updateDeviceRequest.ParentDeviceID;
    public Guid? RackID => updateDeviceRequest.RackID;
    public int? uPosition => updateDeviceRequest.UPosition;
    public string? SlotInParent => updateDeviceRequest.SlotInParent;
    public string? DeviceType => updateDeviceRequest.DeviceType;
    public string? Manufacturer => updateDeviceRequest.Manufacturer;
    public string? SerialNumber => updateDeviceRequest.SerialNumber;
    public string? Model => updateDeviceRequest.Model;
    public DeviceStatus? Status => updateDeviceRequest.Status;
    public string? IpAddress => updateDeviceRequest.IpAddress;
    public int? Size => updateDeviceRequest.Size;
    public string? Name => updateDeviceRequest.Name;
}