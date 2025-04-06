using Rack.Contracts.DataCenter;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

public class UpdateDataCenterCommand(Guid DCId, UpdateDataCenterDto updateDataCenterDto) : ICommand<Response>
{
    public Guid DCId { get; set; } = DCId;
    public string Name { get; set; } = updateDataCenterDto.Name;
    public string? Location { get; set; } = updateDataCenterDto.Location;
}