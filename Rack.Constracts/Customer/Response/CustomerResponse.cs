namespace Rack.Contracts.Customer.Response;

public record CustomerResponse(
    Guid Id,
    string Name,
    string? ContactInfo,
    DateTime CreatedDate
);