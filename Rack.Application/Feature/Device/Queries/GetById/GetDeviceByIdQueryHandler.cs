using Mapster;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Queries.GetById;

internal class GetDeviceByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceByIdQuery, DeviceRackResponse>
{
    public async Task<Response<DeviceRackResponse>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRepository = unitOfWork.GetRepository<Rack.Domain.Entities.Device>();
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
        {
            return Response<DeviceRackResponse>.Failure(Error.NotFound("Không tìm thấy thiết bị này!"));
        }
        var deviceResponse = device.Adapt<DeviceRackResponse>();

        return Response<DeviceRackResponse>.Success(deviceResponse);
    }
}