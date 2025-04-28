namespace Rack.Contracts.PortConnection.Request
{
    public class CreatePortConnectionRequest
    {
        public Guid SourcePortID { get; set; }
        public Guid DestinationPortID { get; set; }
        public string? CableType { get; init; }
        public string? Comment { get; init; }
    }
}