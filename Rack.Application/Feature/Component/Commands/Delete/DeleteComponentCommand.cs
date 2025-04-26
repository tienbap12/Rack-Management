using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.Component.Commands.Delete
{
    public record DeleteComponentCommand(Guid Id) : ICommand<Response>
    {
    }
}