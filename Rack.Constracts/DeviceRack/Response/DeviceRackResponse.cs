using Rack.Contracts.Audit;
using Rack.Contracts.Device.Responses;
using System.Collections;

namespace Rack.Contracts.DeviceRack.Response;

public record DeviceRackResponse : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid DataCenterID { get; init; }
    public string RackNumber { get; init; }
    public int? Size { get; init; }
}