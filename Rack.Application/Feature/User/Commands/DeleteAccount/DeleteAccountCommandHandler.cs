using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Commands.DeleteAccount;

public class DeleteAccountCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<DeleteAccountCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var accountRepo = unitOfWork.GetRepository<Account>();

        // Find the account
        var account = await accountRepo.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            return Response<bool>.Failure(Error.NotFound(message: "Người dùng không tồn tại"));
        }

        // Soft delete the account
        account.IsDeleted = true;
        account.DeletedBy = currentUserService.UserId.ToString();
        account.DeletedOn = DateTime.UtcNow;

        try
        {
            await accountRepo.UpdateAsync(account);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(Error.InternalServerError(message: ex.Message));
        }
    }
}