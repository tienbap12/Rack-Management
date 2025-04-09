using Mapster;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DeviceRack.Queries.GetAll;

internal class GetAllDeviceRackQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDeviceRackQuery, List<DeviceRackResponse>>
{
    public async Task<Response<List<DeviceRackResponse>>> Handle(
        GetAllDeviceRackQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
            var deviceRacks = await deviceRackRepository.GetAllAsync(cancellationToken);

            if (deviceRacks == null || !deviceRacks.Any())
            {
                return Response<List<DeviceRackResponse>>.Success(new List<DeviceRackResponse>());
            }

            var deviceRackResult = deviceRacks.Adapt<List<DeviceRackResponse>>();
            return Response<List<DeviceRackResponse>>.Success(deviceRackResult);
        }
        catch (Exception ex)
        {
            return Response<List<DeviceRackResponse>>.Failure(Error.Failure(ex.Message), Domain.Enum.HttpStatusCodeEnum.InternalServerError);
        }
    }
}