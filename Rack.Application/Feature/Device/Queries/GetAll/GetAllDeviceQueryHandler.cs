using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Queries.GetAll;

internal class GetAllDeviceQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDeviceQuery, List<DeviceResponse>>
{
    public async Task<Response<List<DeviceResponse>>> Handle(
        GetAllDeviceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();

            // Bắt đầu trực tiếp từ BuildQuery trả về IQueryable
            IQueryable<Domain.Entities.Device> query = deviceRepository.BuildQuery; // Giả sử BuildQuery trả về IQueryable<Device>

            // --- Áp dụng bộ lọc động ---

            // 1. Lọc theo RackId nếu được cung cấp
            if (request.Id.HasValue) // Đã đổi tên thành RackId
            {
                // Quy ước: Guid.Empty dùng để tìm device chưa có RackID (unmounted)
                if (request.Id == Guid.Empty)
                {
                    // GÁN LẠI KẾT QUẢ CHO query
                    query = query.Where(d => d.RackID == null);
                }
                else
                {
                    // GÁN LẠI KẾT QUẢ CHO query
                    query = query.Where(d => d.RackID == request.Id.Value);
                }
            }

            // 2. Lọc theo Status nếu được cung cấp
            if (request.Status.HasValue)
            {
                // GÁN LẠI KẾT QUẢ CHO query
                query = query.Where(d => d.Status == request.Status.Value);
            }

            // --- Thực thi truy vấn và lấy kết quả ---
            var filteredDevices = await query.ToListAsync(cancellationToken);

            // --- Ánh xạ sang DTO ---
            var deviceResult = filteredDevices.Adapt<List<DeviceResponse>>();

            return Response<List<DeviceResponse>>.Success(deviceResult);
        }
        catch (Exception ex)
        {
            return Response<List<DeviceResponse>>.Failure(Error.Failure());
        }
    }
}