using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Queries.GetAll;

internal class GetAllComponentQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllComponentQuery, List<ComponentResponse>>
{
    public async Task<Response<List<ComponentResponse>>> Handle(
        GetAllComponentQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var componentRepository = unitOfWork.GetRepository<Domain.Entities.Component>();
            
            // Sử dụng Query để tận dụng AsNoTracking
            var query = componentRepository.Query;

            // Lọc theo Status
            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }

            // Lọc theo StorageRackId
            if (request.StorageRackId.HasValue)
            {
                query = request.StorageRackId == Guid.Empty
                    ? query.Where(c => c.StorageRackID == null && c.Status == ComponentStatus.Available)
                    : query.Where(c => c.StorageRackID == request.StorageRackId.Value);
            }

            // Sử dụng FindAsync thay vì BuildQuery
            var components = await componentRepository.FindAsync(
                c => (request.Status == null || c.Status == request.Status) &&
                     (request.StorageRackId == null || 
                      (request.StorageRackId == Guid.Empty 
                        ? c.StorageRackID == null && c.Status == ComponentStatus.Available
                        : c.StorageRackID == request.StorageRackId)),
                cancellationToken);

            var result = components.Adapt<List<ComponentResponse>>();
            return Response<List<ComponentResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllComponentQueryHandler: {ex}");
            // Log lỗi chi tiết
            return Response<List<ComponentResponse>>.Failure(Error.Failure(), HttpStatusCodeEnum.InternalServerError);
        }
    }
}