namespace Rack.Contracts.ServerRental.Request;

public class CreateServerRentalRequest
{
    public Guid CustomerID { get; set; }
    public Guid DeviceID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}