using Rack.Contracts.Customer.Response;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Customer.Queries.GetAll
{
    public class GetAllCustomerQuery : IQuery<List<CustomerResponse>>
    {
    }
}