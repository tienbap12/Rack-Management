using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;

namespace Rack.Application.Feature.User.Commands.CreateAccount;

public class CreateAccountCommand(CreateAccountRequest request) : ICommand<Response<AccountResponse>>
{
    public CreateAccountRequest Request { get; set; } = request;
}