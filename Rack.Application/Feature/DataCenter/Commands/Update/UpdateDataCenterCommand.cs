using Rack.Contracts.DataCenter.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

public class UpdateDataCenterCommand(Guid DCId, UpdateDataCenterRequest updateDataCenterDto) : ICommand<Response>
{
    public Guid DCId { get; set; } = DCId;
    public string Name { get; set; } = updateDataCenterDto.Name;
    public string? Location { get; set; } = updateDataCenterDto.Location;
}