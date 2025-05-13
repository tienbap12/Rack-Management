using Rack.Contracts.PortConnection.Request;

namespace Rack.Contracts.Port.Request
{
    public class CreatePortRequest
    {
        public Guid DeviceID { get; set; }
        public Guid? CardID { get; set; }
        public string PortName { get; set; }
        public string PortType { get; set; }
        public List<CreatePortConnectionRequest>? PortConnections { get; set; }

    }
}