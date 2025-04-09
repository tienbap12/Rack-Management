using Rack.Contracts.DeviceRack.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DeviceRack.Commands.Create;

public class CreateDeviceRackCommand(CreateDeviceRackRequest createDeviceRackRequest) : ICommand<Response>
{
    public CreateDeviceRackRequest Request { get; set; } = createDeviceRackRequest;
}