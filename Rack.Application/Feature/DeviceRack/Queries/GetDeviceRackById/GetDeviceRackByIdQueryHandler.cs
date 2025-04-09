using Mapster;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DeviceRack.Queries.GetDeviceRackById;

internal class GetDeviceRackByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceRackByIdQuery, DeviceRackResponse>
{
    public async Task<Response<DeviceRackResponse>> Handle(GetDeviceRackByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
        var deviceRack = await deviceRackRepository.GetByIdAsync(request.RackId, cancellationToken);
        if (deviceRack is null)
        {
            return Response<DeviceRackResponse>.Failure(
                Error.NotFound("DeviceRack.NotFound")
            );
        }
        var deviceRackResponse = deviceRack.Adapt<DeviceRackResponse>();
        return Response<DeviceRackResponse>.Success(deviceRackResponse);
    }
}