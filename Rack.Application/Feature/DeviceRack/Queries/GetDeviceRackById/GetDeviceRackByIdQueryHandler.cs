using Mapster;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.DeviceRack.Queries.GetDeviceRackById;

internal class GetDeviceRackByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceRackByIdQuery, DeviceRackResponse>
{
    public async Task<Response<DeviceRackResponse>> Handle(GetDeviceRackByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
        var rackData = await deviceRackRepository.GetByIdAsync(request.RackId, cancellationToken);
        if (rackData == null)
        {
            return Response<DeviceRackResponse>.Failure(Error.NotFound(message:"Không tìm thấy tủ rack này"), HttpStatusCodeEnum.NotFound);
        }
        var rackResult = rackData.Adapt<DeviceRackResponse>();
        return Response<DeviceRackResponse>.Success(rackResult);
    }
}