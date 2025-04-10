using Mapster;
using Rack.Contracts.Customer.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Queries.GetAll
{
    internal class GetAllCustomerQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetAllCustomerQuery, List<CustomerResponse>>
    {
        public async Task<Response<List<CustomerResponse>>> Handle(
            GetAllCustomerQuery request,
            CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();
            var customers = await customerRepo.GetAllAsync(cancellationToken);

            if (customers == null || !customers.Any())
            {
                return Response<List<CustomerResponse>>.Success(new List<CustomerResponse>());
            }

            var customerResult = customers.Adapt<List<CustomerResponse>>();
            return Response<List<CustomerResponse>>.Success(customerResult);
        }
    }
}