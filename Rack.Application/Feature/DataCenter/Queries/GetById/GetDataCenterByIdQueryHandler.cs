using Mapster;
using Rack.Contracts.DataCenter.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Queries.GetById
{
    public class GetDataCenterByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDataCenterByIdQuery, DataCenterResponse>
    {
        public async Task<Response<DataCenterResponse>> Handle(GetDataCenterByIdQuery request, CancellationToken cancellationToken)
        {
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();
            var dataCenter = await dataCenterRepository.GetByIdAsync(request.Id, cancellationToken);
            if (dataCenter == null)
            {
                return Response<DataCenterResponse>.Failure(Error.NotFound());
            }
            var result = dataCenter.Adapt<DataCenterResponse>();
            return Response<DataCenterResponse>.Success(result);
        }
    }
}