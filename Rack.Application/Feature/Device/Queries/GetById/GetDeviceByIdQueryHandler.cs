using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Queries.GetById;

internal class GetDeviceByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceByIdQuery, DeviceResponse>
{
    public async Task<Response<DeviceResponse>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRepository = unitOfWork.GetRepository<Rack.Domain.Entities.Device>();
        var configItemRepository = unitOfWork.GetRepository<Rack.Domain.Entities.ConfigurationItem>();
        var cardRepository = unitOfWork.GetRepository<Rack.Domain.Entities.Card>();
        var portRepository = unitOfWork.GetRepository<Rack.Domain.Entities.Port>();
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
        {
            return Response<DeviceResponse>.Failure(Error.NotFound());
        }
        var listCI = await configItemRepository.BuildQuery.Where(x => x.DeviceID == device.Id).ToListAsync(cancellationToken);
        var listCard = await cardRepository.BuildQuery.Where(x => x.DeviceID == device.Id).ToListAsync(cancellationToken);
        
        var deviceResponse = device.Adapt<DeviceResponse>();

        return Response<DeviceResponse>.Success(deviceResponse);
    }
}