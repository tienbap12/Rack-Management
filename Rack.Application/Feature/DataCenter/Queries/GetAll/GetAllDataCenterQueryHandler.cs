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
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();

            // Truy vấn và ánh xạ dữ liệu
            var dataCenterResult = await dataCenterRepository.BuildQuery
                .Select(dc => new DataCenterResponse
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    Location = dc.Location,
                    CreatedDate = dc.CreatedOn,
                    Racks = dc.Racks
                        .Where(r => !r.IsDeleted)
                        .Select(r => new DeviceRackResponse
                        {
                            Id = r.Id,
                            DataCenterID = r.DataCenterID,
                            RackNumber = r.RackNumber,
                            Size = r.Size,
                            IsDeleted = r.IsDeleted,
                            DeletedOn = r.DeletedOn,
                            DeletedBy = r.DeletedBy,
                            CreatedOn = r.CreatedOn,
                            CreatedBy = r.CreatedBy,
                            LastModifiedOn = r.LastModifiedOn,
                            LastModifiedBy = r.LastModifiedBy
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Response<List<DataCenterResponse>>.Success(dataCenterResult);
        }
        catch (Exception ex)
        {
            return Response<List<DataCenterResponse>>.Failure(
                Error.Failure()
            );
        }
    }
}