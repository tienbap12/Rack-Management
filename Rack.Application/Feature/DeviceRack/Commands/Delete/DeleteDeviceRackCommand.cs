using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DeviceRack.Commands.Delete;

public class DeleteDeviceRackCommand(Guid DeviceRackId) : ICommand<Response>
{
    public Guid DeviceRackId { get; set; } = DeviceRackId;
}