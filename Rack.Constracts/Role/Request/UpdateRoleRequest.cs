using Rack.Domain.Enum;

namespace Rack.Contracts.Role.Requests;

public record UpdateRoleRequest(
    string Name,
    CommonStatus Status
);