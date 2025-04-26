using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.DataCenter.Response;
using Rack.Contracts.Device.Responses;
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
            var rackData = await deviceRackRepository.GetAllAsync(cancellationToken);
            var rackResult = rackData.Adapt<List<DeviceRackResponse>>();
            return Response<List<DeviceRackResponse>>.Success(rackResult);
        }
        catch (Exception ex)
        {
            return Response<List<DeviceRackResponse>>.Failure(Error.Failure());
        }
    }
}