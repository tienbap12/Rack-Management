using Rack.Contracts.Customer.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Customer.Queries.GetById
{
    public class GetCustomerByIdQuery(Guid customerId) : IQuery<CustomerResponse>
    {
        public Guid CustomerId { get; set; } = customerId;
    }
}