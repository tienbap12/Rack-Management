using Rack.Contracts.DataCenter.Requests;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Create;

public class CreateDataCenterCommand(CreateDataCenterRequest createDataCenterDto) : ICommand<Response>
{
    public string Name { get; set; } = createDataCenterDto.Name;
    public string Location { get; set; } = createDataCenterDto.Location;
}