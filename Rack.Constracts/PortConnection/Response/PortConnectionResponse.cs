using Rack.Contracts.Audit;

namespace Rack.Contracts.PortConnection.Response
{
    public record PortConnectionResponse : BaseAuditDto
    {
        public Guid SourcePortID { get; init; }
        public Guid DestinationPortID { get; init; }
        public string? CableType { get; init; }
        public string? Comment { get; init; }
    }
}