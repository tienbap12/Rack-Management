using Mapster;
using Rack.Contracts.Customer.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Queries.GetById
{
    public class GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
    {
        public async Task<Response<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();
            var customer = await customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);

            if (customer == null)
            {
                return Response<CustomerResponse>.Failure(Error.NotFound());
            }

            var result = customer.Adapt<CustomerResponse>();
            return Response<CustomerResponse>.Success(result);
        }
    }
}