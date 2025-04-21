using Rack.Contracts.Audit;

namespace Rack.Contracts.ServerRental.Response;

public record ServerRentalResponse
 : BaseAuditDto
{
    public Guid Id { get; init; }
    public Guid CustomerID { get; init; }
    public Guid DeviceID { get; init; }
    public string CustomerName { get; init; }
    public string DeviceName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}