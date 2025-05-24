using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Abstractions;
using Rack.Application.Commons.Interfaces;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.User.Commands.UpdateAccount;

public class UpdateAccountCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IUserContext currentUserService
) : ICommandHandler<UpdateAccountCommand, Response<AccountResponse>>
{
    public async Task<Response<AccountResponse>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountRepo = unitOfWork.GetRepository<Account>();
        var roleRepo = unitOfWork.GetRepository<Domain.Entities.Role>();

        // Find the account
        var account = await accountRepo.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            return Response<AccountResponse>.Failure(Error.NotFound(message: "Người dùng không tồn tại"));
        }

        // Check if email exists (if changed)
        if (request.Request.Email != null && account.Email != request.Request.Email)
        {
            var existingAccount = await accountRepo.BuildQuery
                .Where(a => a.Email == request.Request.Email && a.Id != request.AccountId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAccount != null)
            {
                return Response<AccountResponse>.Failure(Error.Conflict(message: "Email đã tồn tại"));
            }

            account.Email = request.Request.Email;
        }

        // Update other fields if provided
        if (request.Request.FullName != null)
        {
            account.FullName = request.Request.FullName;
        }

        if (request.Request.DoB.HasValue)
        {
            account.DoB = request.Request.DoB;
        }

        if (request.Request.Phone != null)
        {
            account.Phone = request.Request.Phone;
        }

        if (request.Request.RoleId.HasValue)
        {
            var role = await roleRepo.GetByIdAsync(request.Request.RoleId.Value, cancellationToken);
            if (role == null)
            {
                return Response<AccountResponse>.Failure(Error.NotFound(message: "Vai trò không tồn tại"));
            }

            account.RoleId = request.Request.RoleId.Value;
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(request.Request.NewPassword))
        {
            var (hashedPassword, salt) = passwordHasher.HashPassword(request.Request.NewPassword);
            account.Password = hashedPassword;
            account.Salt = salt;
        }

        // Update audit info
        account.LastModifiedBy = currentUserService.GetUsername();
        account.LastModifiedOn = DateTime.UtcNow;

        try
        {
            await accountRepo.UpdateAsync(account, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Get updated account with role
            var updatedAccount = await accountRepo.BuildQuery
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            // Return the updated account
            var response = new AccountResponse
            {
                Id = updatedAccount.Id,
                Username = updatedAccount.Username,
                FullName = updatedAccount.FullName,
                DoB = updatedAccount.DoB,
                Phone = updatedAccount.Phone,
                Email = updatedAccount.Email,
                RoleName = updatedAccount.Role.Name,
                RoleId = updatedAccount.RoleId,
                CreatedOn = updatedAccount.CreatedOn,
                LastModifiedOn = updatedAccount.LastModifiedOn,
                IsDeleted = updatedAccount.IsDeleted
            };

            return Response<AccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Response<AccountResponse>.Failure(Error.InternalServerError(message: ex.Message));
        }
    }
}