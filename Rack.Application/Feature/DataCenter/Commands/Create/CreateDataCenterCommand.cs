using Rack.Contracts.DataCenter;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Create;

public class CreateDataCenterCommand(CreateDataCenterDto createDataCenterDto) : ICommand<Response>
{
    public string Name { get; set; } = createDataCenterDto.Name;
    public string Location { get; set; } = createDataCenterDto.Location;
}