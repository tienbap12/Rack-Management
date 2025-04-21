using Rack.Contracts.Audit;

namespace Rack.Contracts.Role.Responses;
public record RoleResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Status { get; init; }
}