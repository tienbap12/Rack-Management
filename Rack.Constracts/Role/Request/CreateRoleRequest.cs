namespace Rack.Contracts.Role.Requests;

public record CreateRoleRequest(
    string Name,
    string Status = "Active"
);