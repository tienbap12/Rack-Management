using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ServerRental.Commands.Delete
{
    public class DeleteServerRentalCommand(Guid rentalId) : ICommand<Response>
    {
        public Guid RentalId { get; set; } = rentalId;
    }
}