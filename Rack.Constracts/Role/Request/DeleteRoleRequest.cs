namespace Rack.Contracts.Role.Requests;

public record DeleteRoleRequest(
    Guid Id,
    string DeletedBy = "System"
); 