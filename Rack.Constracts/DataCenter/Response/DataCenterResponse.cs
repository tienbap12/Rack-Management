namespace Rack.Contracts.DataCenter.Responses;

public record DataCenterResponse(
    Guid Id,
    string Name,
    string? Location,
    DateTime CreatedDate,
    DateTime? LastModifiedDate
);