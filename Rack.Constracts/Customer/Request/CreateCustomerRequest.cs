namespace Rack.Contracts.Customer.Request;

public record CreateCustomerRequest(
    string Name,
    string? ContactInfo
);