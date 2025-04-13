using Mapster;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetById;

public class GetConfigurationItemByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetConfigurationItemByIdQuery, ConfigurationItemResponse>
{
    public async Task<Response<ConfigurationItemResponse>> Handle(GetConfigurationItemByIdQuery request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var configItem = await configItemRepo.GetByIdAsync(request.Id, cancellationToken);

        if (configItem == null || configItem.IsDeleted)
        {
            return Response<ConfigurationItemResponse>.Failure(Error.NotFound("Cấu hình không tồn tại"));
        }

        var result = configItem.Adapt<ConfigurationItemResponse>();
        return Response<ConfigurationItemResponse>.Success(result);
    }
}