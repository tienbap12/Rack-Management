namespace Rack.Contracts.Role.Requests;

public record UpdateRoleRequest(
    string Name,
    string Status,
    string LastModifiedBy = "System"
);