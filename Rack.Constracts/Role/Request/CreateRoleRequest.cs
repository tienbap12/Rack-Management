using Rack.Domain.Enum;

namespace Rack.Contracts.Role.Requests;

public record CreateRoleRequest(
    string Name,
    CommonStatus Status
);