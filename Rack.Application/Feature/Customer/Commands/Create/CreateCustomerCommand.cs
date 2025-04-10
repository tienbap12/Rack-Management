using Rack.Contracts.Customer.Request;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Customer.Commands.Create;

public class CreateCustomerCommand(CreateCustomerRequest createCustomerDto) : ICommand<Response>
{
    public string Name { get; set; } = createCustomerDto.Name;
    public string? ContactInfo { get; set; } = createCustomerDto.ContactInfo;
}