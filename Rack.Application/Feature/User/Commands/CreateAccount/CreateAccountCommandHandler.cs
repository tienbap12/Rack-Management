using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using System;
using System.Security.Claims;

namespace Rack.Application.Feature.User.Commands.CreateAccount;

public class CreateAccountCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService
) : ICommandHandler<CreateAccountCommand, Response<AccountResponse>>
{
    public async Task<Response<AccountResponse>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountRepo = unitOfWork.GetRepository<Account>();
        var roleRepo = unitOfWork.GetRepository<Domain.Entities.Role>();

        // Check if the role exists
        var role = await roleRepo.GetByIdAsync(request.Request.RoleId, cancellationToken);
        if (role == null)
        {
            return Response<AccountResponse>.Failure(Error.NotFound(message: "Vai trò không tồn tại"));
        }

        // Check if username or email already exists
        var existingAccount = await accountRepo.BuildQuery
            .Where(a => a.Username == request.Request.Username || a.Email == request.Request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAccount != null)
        {
            if (existingAccount.Username == request.Request.Username)
            {
                return Response<AccountResponse>.Failure(Error.Conflict(message: "Tên đăng nhập đã tồn tại"));
            }
            else
            {
                return Response<AccountResponse>.Failure(Error.Conflict(message: "Email đã tồn tại"));
            }
        }

        // Hash the password
        var (hashedPassword, salt) = passwordHasher.HashPassword(request.Request.Password);

        // Create the account
        var account = new Account
        {
            Username = request.Request.Username,
            Password = hashedPassword,
            Salt = salt,
            FullName = request.Request.FullName,
            DoB = request.Request.DoB,
            Phone = request.Request.Phone,
            Email = request.Request.Email,
            RoleId = request.Request.RoleId,
            CreatedBy = currentUserService.Username,
            CreatedOn = DateTime.UtcNow
        };

        try
        {
            await accountRepo.CreateAsync(account, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Return the created account
            var response = new AccountResponse
            {
                Id = account.Id,
                Username = account.Username,
                FullName = account.FullName,
                DoB = account.DoB,
                Phone = account.Phone,
                Email = account.Email,
                RoleName = role.Name,
                RoleId = account.RoleId,
                CreatedOn = account.CreatedOn,
                LastModifiedOn = account.LastModifiedOn,
                IsDeleted = account.IsDeleted
            };

            return Response<AccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Response<AccountResponse>.Failure(Error.InternalServerError(message: ex.Message));
        }
    }
}