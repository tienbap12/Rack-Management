using Mapster;
using Rack.Contracts.ServerRental.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ServerRental.Queries.GetAll
{
    internal class GetAllServerRentalQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetAllServerRentalQuery, List<ServerRentalResponse>>
    {
        public async Task<Response<List<ServerRentalResponse>>> Handle(
            GetAllServerRentalQuery request,
            CancellationToken cancellationToken)
        {
            var serverRentalRepo = unitOfWork.GetRepository<Domain.Entities.ServerRental>();
            var serverRentals = await serverRentalRepo.GetAllAsync(cancellationToken);

            if (serverRentals == null || !serverRentals.Any())
            {
                return Response<List<ServerRentalResponse>>.Success(new List<ServerRentalResponse>());
            }

            var serverRentalResult = serverRentals.Adapt<List<ServerRentalResponse>>();
            return Response<List<ServerRentalResponse>>.Success(serverRentalResult);
        }
    }
}