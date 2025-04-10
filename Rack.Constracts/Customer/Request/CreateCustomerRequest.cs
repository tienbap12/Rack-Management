namespace Rack.Contracts.Customer.Request;

public class CreateCustomerRequest
{
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
}