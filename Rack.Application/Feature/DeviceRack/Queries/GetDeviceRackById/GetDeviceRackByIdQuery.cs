using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;

public class GetDeviceRackByIdQuery(Guid rackId) : IQuery<DeviceRackResponse>
{
    public Guid RackId { get; set; } = rackId;
}