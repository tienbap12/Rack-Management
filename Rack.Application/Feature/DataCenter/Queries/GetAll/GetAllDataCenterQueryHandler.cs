using Microsoft.EntityFrameworkCore;
using Rack.Contracts.DataCenter.Response;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Queries.GetAll;

internal class GetAllDataCenterQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDataCenterQuery, List<DataCenterResponse>>
{
    public async Task<Response<List<DataCenterResponse>>> Handle(
        GetAllDataCenterQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy repository cho DataCenter
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();
            var rackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

            // Lấy toàn bộ DataCenter từ database
            var dataCenters = await dataCenterRepository.GetAllAsync(cancellationToken);

            // Kiểm tra dữ liệu trả về
            if (dataCenters == null || !dataCenters.Any())
            {
                return Response<List<DataCenterResponse>>.Success(new List<DataCenterResponse>());
            }
            var dataCenterResult = await dataCenterRepository.BuildQuery.Select(dc => new DataCenterResponse
            {
                Id = dc.Id,
                Name = dc.Name,
                Location = dc.Location,
                CreatedDate = dc.CreatedOn,
                Racks = dc.Racks
                              .Where(r => !r.IsDeleted) // QUAN TRỌNG: Chỉ lấy các Rack chưa bị xóa
                              .Select(r => new DeviceRackResponse // Chiếu DeviceRack sang DeviceRackResponse
                              {
                                  Id = r.Id,
                                  DataCenterID = r.DataCenterID, // Gán nếu cần trong response
                                  RackNumber = r.RackNumber,
                                  Size = r.Size,
                                  CreatedOn = r.CreatedOn // Ánh xạ từ CreatedOn của Rack
                              })
                              .ToList()
            }).ToListAsync(cancellationToken);

            // Trả về response thành công
            return Response<List<DataCenterResponse>>.Success(dataCenterResult);
        }
        catch (Exception ex)
        {
            // Xử lý lỗi và trả về response thất bại
            return Response<List<DataCenterResponse>>.Failure(Error.Failure(ex.Message), Domain.Enum.HttpStatusCodeEnum.InternalServerError);
        }
    }
}