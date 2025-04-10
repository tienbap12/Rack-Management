using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Customer.Commands.Delete
{
    public class DeleteCustomerCommand(Guid customerId) : ICommand<Response>
    {
        public Guid CustomerId { get; set; } = customerId;
    }
}