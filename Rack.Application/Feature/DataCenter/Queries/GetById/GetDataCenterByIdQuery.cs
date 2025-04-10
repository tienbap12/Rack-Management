using Rack.Contracts.DataCenter.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DataCenter.Queries.GetById
{
    public class GetDataCenterByIdQuery(Guid dcID) : IQuery<DataCenterResponse>
    {
        public Guid Id { get; set; } = dcID;
    }
}