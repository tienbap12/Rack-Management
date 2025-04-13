namespace Rack.Contracts.DeviceRack.Response;

public record DeviceRackResponse(
 Guid Id,
 Guid DataCenterID,
 string RackNumber,
 string? Size,
 bool IsDeleted,
 DateTime? DeletedOn,
 string? DeletedBy,
 DateTime CreatedOn,
 string CreatedBy,
 DateTime? LastModifiedOn,
 string LastModifiedBy
);