using Mapster;
using Rack.Contracts.DataCenter.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Primitives;

namespace Rack.Application.Feature.DataCenter.Queries.GetAll;

internal class GetAllDataCenterQuerryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDataCenterQuerry, List<DataCenterResponse>>
{
    public async Task<Response<List<DataCenterResponse>>> Handle(
        GetAllDataCenterQuerry request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy repository cho DataCenter
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();

            // Lấy toàn bộ DataCenter từ database
            var dataCenters = await dataCenterRepository.GetAllAsync(cancellationToken);

            // Kiểm tra dữ liệu trả về
            if (dataCenters == null || !dataCenters.Any())
            {
                return Response<List<DataCenterResponse>>.Success(new List<DataCenterResponse>());
            }
            var dataCenterResult = dataCenters.Adapt<List<DataCenterResponse>>();
            // Trả về response thành công
            return Response<List<DataCenterResponse>>.Success(dataCenterResult);
        }
        catch (Exception ex)
        {
            // Xử lý lỗi và trả về response thất bại
            return Response<List<DataCenterResponse>>.Failure(ex.Message);
        }
    }
}