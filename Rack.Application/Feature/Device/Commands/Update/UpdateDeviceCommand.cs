using Rack.Contracts.Device.Requests;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Device.Commands.Update;

public class UpdateDeviceCommand(Guid deviceId, UpdateDeviceRequest request)
    : ICommand<Response<DeviceResponse>>
{
    public Guid DeviceId { get; } = deviceId;
    public UpdateDeviceRequest Request { get; } = request;
}