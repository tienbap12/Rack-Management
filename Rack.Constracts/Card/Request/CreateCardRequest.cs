using Rack.Contracts.Port.Request;
using Rack.Contracts.PortConnection.Request;

namespace Rack.Contracts.Card.Request
{
    public class CreateCardRequest
    {
        public Guid Id { get; set; }
        public Guid DeviceID { get; set; }
        public string CardType { get; init; }
        public string CardName { get; init; }
        public string? SerialNumber { get; set; }
        public List<CreatePortRequest>? Ports { get; set; }
    }
}