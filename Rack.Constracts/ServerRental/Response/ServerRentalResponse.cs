namespace Rack.Contracts.ServerRental.Response;

public class ServerRentalResponse
{
    public Guid Id { get; set; }
    public Guid CustomerID { get; set; }
    public Guid DeviceID { get; set; }
    public string CustomerName { get; set; } = null!; // Assuming from Customer
    public string DeviceName { get; set; } = null!;   // Assuming from Device
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}