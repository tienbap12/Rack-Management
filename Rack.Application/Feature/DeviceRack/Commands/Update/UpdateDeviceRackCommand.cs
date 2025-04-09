using Rack.Contracts.DeviceRack.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DeviceRack.Commands.Update;

public class UpdateDeviceRackCommand(Guid rackId, UpdateDeviceRackRequest updateDeviceRackRequest) : ICommand<Response>
{
    public Guid RackId { get; set; } = rackId;
    public Guid DataCenterID { get; set; } = updateDeviceRackRequest.DataCenterID;
    public string RackNumber { get; set; } = updateDeviceRackRequest.RackNumber;
    public string? Size { get; set; } = updateDeviceRackRequest.Size;
}