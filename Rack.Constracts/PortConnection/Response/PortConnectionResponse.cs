using Rack.Contracts.Audit;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.Port.Response;

namespace Rack.Contracts.PortConnection.Response
{
    public record PortConnectionResponse : BaseAuditDto
    {
        public Guid Id { get; init; }
        public Guid SourcePortID { get; init; }
        public Guid DestinationPortID { get; init; }
        public string? CableType { get; init; }
        public string? Comment { get; init; }

        // Source device information
        public SimpleDeviceDto? SourceDevice { get; init; }
        public PortResponse? SourcePort { get; init; }

        // Destination device information
        public SimpleDeviceDto? DestinationDevice { get; init; }
        public PortResponse? DestinationPort { get; init; }

        // Thông tin đầu bên kia của kết nối (remote)
        public SimpleDeviceDto? RemoteDevice { get; init; }
        public PortResponse? RemotePort { get; init; }
    }

    public record SimpleDeviceDto
    {
        public Guid Id { get; init; }
        public string? RackName { get; init; }
        public string? Slot { get; init; }
        public string DeviceName { get; init; }
    }
}