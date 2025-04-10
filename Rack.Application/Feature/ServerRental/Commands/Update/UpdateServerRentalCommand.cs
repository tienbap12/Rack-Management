using Rack.Contracts.ServerRental.Request;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ServerRental.Commands.Update
{
    public class UpdateServerRentalCommand(Guid rentalId, UpdateServerRentalRequest updateServerRentalDto) : ICommand<Response>
    {
        public Guid RentalId { get; set; } = rentalId;
        public DateTime StartDate { get; set; } = updateServerRentalDto.StartDate;
        public DateTime? EndDate { get; set; } = updateServerRentalDto.EndDate;
    }
}