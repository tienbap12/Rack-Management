using Rack.Contracts.Customer.Request;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Customer.Commands.Update;

public class UpdateCustomerCommand(Guid customerId, UpdateCustomerRequest updateCustomerDto) : ICommand<Response>
{
    public Guid CustomerId { get; set; } = customerId;
    public string Name { get; set; } = updateCustomerDto.Name;
    public string? ContactInfo { get; set; } = updateCustomerDto.ContactInfo;
}