namespace Rack.Contracts.Customer.Request;

public record UpdateCustomerRequest(
    string Name,
    string? ContactInfo
);