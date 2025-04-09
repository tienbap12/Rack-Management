namespace Rack.Contracts.DeviceRack.Response
{
    public class DeviceRackResponse
    {
        public Guid DataCenterID { get; set; }
        public string RackNumber { get; set; } = null!;
        public string? Size { get; set; }    // Ví dụ: '42U'
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
    }
}