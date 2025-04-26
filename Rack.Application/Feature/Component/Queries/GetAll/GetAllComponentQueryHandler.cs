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
            IQueryable<Domain.Entities.Component> query = componentRepository.BuildQuery; // Giả sử có BuildQuery

            // Lọc theo Status
            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }

            // Lọc theo StorageRackId
            if (request.StorageRackId.HasValue)
            {
                // Quy ước Guid.Empty để tìm component chưa có StorageRackID? Cân nhắc lại.
                // Nên để null hoặc thêm flag IsUnstored.
                if (request.StorageRackId == Guid.Empty)
                {
                    query = query.Where(c => c.StorageRackID == null && c.Status == ComponentStatus.Available); // Chỉ có ý nghĩa nếu status là Available
                }
                else
                {
                    query = query.Where(c => c.StorageRackID == request.StorageRackId.Value);
                }
            }

            // Lọc theo CurrentDeviceId
            if (request.CurrentDeviceId.HasValue)
            {
                // Quy ước Guid.Empty để tìm component chưa gắn vào device? Không hợp lý lắm.
                // Nên lọc theo Status = InUse và CurrentDeviceId cụ thể.
                if (request.CurrentDeviceId != Guid.Empty) // Bỏ qua Guid.Empty ở đây
                {
                    query = query.Where(c => c.CurrentDeviceID == request.CurrentDeviceId.Value);
                }
            }

            // Lọc xóa mềm (trừ khi request yêu cầu khác)
            // if (request.IncludeDeleted != true)
            // {
            query = query.Where(c => !c.IsDeleted);
            // }

            var components = await query.ToListAsync(cancellationToken);
            var componentResult = components.Adapt<List<ComponentResponse>>();

            return Response<List<ComponentResponse>>.Success(componentResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllComponentQueryHandler: {ex}");
            // Log lỗi chi tiết
            return Response<List<ComponentResponse>>.Failure(Error.Failure(), HttpStatusCodeEnum.InternalServerError);
        }
    }
}