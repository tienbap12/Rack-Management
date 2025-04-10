namespace Rack.Contracts.Customer.Request;

public class UpdateCustomerRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
}