using Rack.Domain.Commons.Primitives;
using Rack.Domain.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Delete;

public class DeleteDataCenterCommand(Guid DCId) : ICommand<Response>
{
    public Guid DCId { get; set; } = DCId;
}