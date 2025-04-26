using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.ConfigurationItem.Queries.GetById;

public class GetConfigurationItemByDeviceIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetConfigurationItemByDeviceIdQuery, List<ConfigurationItemResponse>>
{
    public async Task<Response<List<ConfigurationItemResponse>>> Handle(GetConfigurationItemByDeviceIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var configItemRepository = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>(); // Vẫn cần để kiểm tra Device tồn tại

            // 1. Kiểm tra xem Device có tồn tại không (vẫn nên làm)
            var deviceExists = await deviceRepository.BuildQuery.AnyAsync(d => d.Id == request.Id);
            if (!deviceExists)
            {
                // Trả về lỗi NotFound cho Device
                return Response<List<ConfigurationItemResponse>>.Failure(Error.NotFound(message: $"Không tìm thấy thiết bị với Id {request.Id}"), HttpStatusCodeEnum.NotFound);
            }

            // 2. Truy vấn danh sách ConfigurationItems theo DeviceId
            // Sử dụng BuildQuery và Where để lọc ở DB
            var configItems = await configItemRepository.BuildQuery // Giả sử có BuildQuery trả về IQueryable<ConfigurationItem>
                                        .Where(ci => ci.DeviceID == request.Id) // Lọc theo DeviceId và IsDeleted
                                        .ToListAsync(cancellationToken);

            // 3. Map kết quả sang List<ConfigurationItemResponse>
            // Mapster tự động xử lý map List sang List nếu cấu hình đúng cho item type
            var result = configItems.Adapt<List<ConfigurationItemResponse>>();

            // 4. Trả về thành công (ngay cả khi danh sách rỗng)
            return Response<List<ConfigurationItemResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetConfigurationItemsByDeviceIdQueryHandler: {ex}");
            // Log lỗi
            return Response<List<ConfigurationItemResponse>>.Failure(Error.Failure(message: "An error occurred while fetching configuration items."), HttpStatusCodeEnum.InternalServerError);
        }
    }
}