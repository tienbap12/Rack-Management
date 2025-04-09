namespace Rack.Contracts.Role.Responses;

public record RoleResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedOn,
    DateTime? LastModifiedOn,
    string CreatedBy,
    string LastModifiedBy
); 