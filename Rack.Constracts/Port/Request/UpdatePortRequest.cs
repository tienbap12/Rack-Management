namespace Rack.Contracts.Port.Request
{
    public class UpdatePortRequest
    {
        public Guid DeviceID { get; set; }
        public Guid? CardID { get; set; }
        public string? PortName { get; init; }
        public string? PortType { get; init; }
    }
}