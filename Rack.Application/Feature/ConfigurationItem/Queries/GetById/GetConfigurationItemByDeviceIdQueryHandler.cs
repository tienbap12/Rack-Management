using Mapster;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetById;

public class GetConfigurationItemByDeviceIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetConfigurationItemByDeviceIdQuery, ConfigurationItemResponse>
{
    public async Task<Response<ConfigurationItemResponse>> Handle(GetConfigurationItemByDeviceIdQuery request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();

        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (device == null)
            return Response<ConfigurationItemResponse>.Failure(Error.NotFound("Thiết bị không tồn tại"));
        var configItem = await configItemRepo.GetByIdAsync(request.Id, cancellationToken);

        if (configItem == null || configItem.IsDeleted)
        {
            return Response<ConfigurationItemResponse>.Failure(Error.NotFound("Cấu hình không tồn tại"));
        }

        var result = configItem.Adapt<ConfigurationItemResponse>();
        return Response<ConfigurationItemResponse>.Success(result);
    }
}