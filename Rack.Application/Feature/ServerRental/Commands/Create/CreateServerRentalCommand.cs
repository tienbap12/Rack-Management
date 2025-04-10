using Rack.Contracts.ServerRental.Request;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.ServerRental.Commands.Create
{
    public class CreateServerRentalCommand(CreateServerRentalRequest createServerRentalDto) : ICommand<Response>
    {
        public Guid CustomerID { get; set; } = createServerRentalDto.CustomerID;
        public Guid DeviceID { get; set; } = createServerRentalDto.DeviceID;
        public DateTime StartDate { get; set; } = createServerRentalDto.StartDate;
        public DateTime? EndDate { get; set; } = createServerRentalDto.EndDate;
    }
}