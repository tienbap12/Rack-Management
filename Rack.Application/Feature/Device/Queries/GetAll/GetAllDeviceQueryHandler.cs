using Mapster;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Queries.GetAll;

internal class GetAllDeviceQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDeviceQuery, List<DeviceResponse>>
{
    public async Task<Response<List<DeviceResponse>>> Handle(
        GetAllDeviceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
            var devices = await deviceRepository.GetAllAsync(cancellationToken);

            if (devices == null || !devices.Any())
            {
                return Response<List<DeviceResponse>>.Success(new List<DeviceResponse>());
            }

            var deviceResult = devices.Adapt<List<DeviceResponse>>();
            return Response<List<DeviceResponse>>.Success(deviceResult);
        }
        catch (Exception ex)
        {
            return Response<List<DeviceResponse>>.Failure(Error.Failure(ex.Message), Domain.Enum.HttpStatusCodeEnum.InternalServerError);
        }
    }
}