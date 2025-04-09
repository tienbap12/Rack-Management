using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Device.Commands.Delete;

public class DeleteDeviceCommand(Guid Id) : ICommand<Response>
{
    public Guid Id { get; set; } = Id;
}