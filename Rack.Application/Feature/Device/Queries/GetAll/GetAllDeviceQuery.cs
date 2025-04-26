using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Queries.GetAll;

public class GetAllDeviceQuery(Guid? rackId, DeviceStatus? status) : IQuery<List<DeviceResponse>>
{
    public Guid? Id { get; set; } = rackId;
    public DeviceStatus? Status { get; set; } = status;
}