using Rack.Contracts.Component.Request;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Commands.Update
{
    public class UpdateComponentCommand(Guid Id, UpdateComponentRequest Request) : ICommand<Response>
    {
        public Guid Id { get; } = Id;
        public string? Name => Request.Name;
        public string? ComponentType => Request.ComponentType;
        public ComponentStatus? Status => Request.Status;
        public Guid? StorageRackId => Request.StorageRackId;
        public Guid? CurrentDeviceId => Request.CurrentDeviceId;
        public string? Manufacturer => Request.Manufacturer;
        public string? Model => Request.Model;
        public string? SpecificationDetails => Request.SpecificationDetails;
    }
}