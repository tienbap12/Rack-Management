using Rack.Contracts.Audit;

namespace Rack.Contracts.Port.Response
{
    public record PortResponse : BaseAuditDto
    {
        public Guid DeviceID { get; init; }
        public Guid? CardID { get; init; }
        public string PortName { get; init; } = null!;
        public string PortType { get; init; } = null!;
    }
}