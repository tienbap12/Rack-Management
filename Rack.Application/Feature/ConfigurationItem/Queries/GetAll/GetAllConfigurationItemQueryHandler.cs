using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetAll;

public class GetAllConfigurationItemQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetAllConfigurationItemQuery, List<ConfigurationItemResponse>>
{
    public async Task<Response<List<ConfigurationItemResponse>>> Handle(GetAllConfigurationItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
            var configItems = await configItemRepo.BuildQuery
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);

            if (configItems == null || !configItems.Any())
            {
                return Response<List<ConfigurationItemResponse>>.Success(new List<ConfigurationItemResponse>());
            }

            var result = configItems.Adapt<List<ConfigurationItemResponse>>();
            return Response<List<ConfigurationItemResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<List<ConfigurationItemResponse>>.Failure(
                Error.Failure(ex.Message),
                Domain.Enum.HttpStatusCodeEnum.InternalServerError);
        }
    }
}