namespace Rack.Contracts.Port.Request
{
    public class CreatePortRequest
    {
        public Guid DeviceID { get; set; }
        public Guid? CardID { get; set; }
        public string PortName { get; set; }
        public string PortType { get; set; }
    }
}