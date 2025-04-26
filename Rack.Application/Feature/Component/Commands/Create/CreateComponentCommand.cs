using Rack.Contracts.Component.Request;
using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Commands.Create
{
    public class CreateComponentCommand(CreateComponentRequest Request) : ICommand<Response>
    {
        public string Name => Request.Name;
        public string ComponentType => Request.ComponentType;
        public ComponentStatus Status => Request.Status;
        public string? SerialNumber => Request.SerialNumber;
        public Guid? StorageRackId => Request.StorageRackId;
        public string? Manufacturer => Request.Manufacturer;
        public string? Model => Request.Model;
        public string? SpecificationDetails => Request.SpecificationDetails;
    }
}