using Microsoft.EntityFrameworkCore;
using Rack.Application.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using Rack.Domain.Enumerations;

namespace Rack.Application.Feature.User.Commands.Register;

internal class RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : ICommandHandler<RegisterCommand, Response>
{
    public async Task<Response> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userRepo = unitOfWork.GetRepository<Account>();
        var existingUser = await userRepo.BuildQuery.Where(x => x.Email == request.Request.Email).FirstOrDefaultAsync(cancellationToken);
        var roleRepo = unitOfWork.GetRepository<Domain.Entities.Role>();
        var role = await roleRepo.GetByIdAsync(request.Request.RoleId, cancellationToken);

        if (role == null)
        {
            return Response.Failure(Error.NotFound("Quyền này không tồn tại"));
        }
        if (existingUser != null)
        {
            return Response.Failure(Error.Conflict("Địa chỉ Email đã tồn tại"));
        }

        var (hashedPassword, salt) = passwordHasher.HashPassword(request.Request.Password);

        var newUser = new Account
        {
            Username = request.Request.Username,
            Password = hashedPassword,
            Salt = salt,
            Email = request.Request.Email,
            FullName = request.Request.FullName,
            Phone = request.Request.Phone,
            RoleId = request.Request.RoleId,
            DoB = request.Request.DoB,
        };

        try
        {
            await userRepo.CreateAsync(newUser, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Response.Success("Tạo tài khoản thành công");
        }
        catch (Exception ex)
        {
            return Response.Failure(Error.Conflict(ex.ToString()));
        }
    }
}