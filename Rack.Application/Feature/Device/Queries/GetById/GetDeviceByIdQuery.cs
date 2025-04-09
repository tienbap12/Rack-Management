using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Device.Queries.GetById;

public class GetDeviceByIdQuery(Guid deviceId) : IQuery<DeviceRackResponse>
{
    public Guid Id { get; set; } = deviceId;
}