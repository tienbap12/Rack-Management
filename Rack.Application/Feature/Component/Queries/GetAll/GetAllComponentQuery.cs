using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Queries.GetAll
{
    public class GetAllComponentQuery(
    ComponentStatus? Status,
    Guid? StorageRackId,
    Guid? CurrentDeviceId) : IQuery<List<ComponentResponse>>
    {
        public ComponentStatus? Status { get; } = Status;
        public Guid? StorageRackId { get; } = StorageRackId;
        public Guid? CurrentDeviceId { get; } = CurrentDeviceId;
    }
}