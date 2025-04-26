using Rack.Contracts.Audit;
using Rack.Domain.Enum;

namespace Rack.Contracts.Role.Responses;
public record RoleResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public CommonStatus Status { get; init; }
}