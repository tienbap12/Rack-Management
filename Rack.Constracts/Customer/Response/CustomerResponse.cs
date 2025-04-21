using Rack.Contracts.Audit;

namespace Rack.Contracts.Customer.Response;

public record CustomerResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string? ContactInfo { get; init; }
}