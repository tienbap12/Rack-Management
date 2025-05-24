using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.DeleteAccount;
 
public class DeleteAccountCommand(Guid accountId) : ICommand<Response<bool>>
{
    public Guid AccountId { get; set; } = accountId;
}