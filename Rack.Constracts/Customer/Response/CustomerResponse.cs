namespace Rack.Contracts.Customer.Response;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public DateTime CreatedDate { get; set; }

    // Optional: Thêm số lượng hợp đồng thuê server nếu cần
    public int ServerRentalCount { get; set; }
}