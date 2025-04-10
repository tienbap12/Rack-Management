using Rack.Contracts.ServerRental.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ServerRental.Queries.GetById
{
    public class GetServerRentalByIdQuery(Guid rentalId) : IQuery<ServerRentalResponse>
    {
        public Guid RentalId { get; set; } = rentalId;
    }
}