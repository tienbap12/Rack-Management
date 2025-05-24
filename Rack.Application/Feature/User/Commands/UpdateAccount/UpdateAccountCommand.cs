using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.UpdateAccount;
 
public class UpdateAccountCommand(Guid accountId, UpdateAccountRequest request) : ICommand<Response<AccountResponse>>
{
    public Guid AccountId { get; set; } = accountId;
    public UpdateAccountRequest Request { get; set; } = request;
}