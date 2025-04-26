using Microsoft.EntityFrameworkCore;
using Rack.Application.Commons.Abstractions;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Commands.Register;

internal class RegisterCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider
) : ICommandHandler<RegisterCommand, Response<AuthResponse>>
{
    public async Task<Response<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userRepo = unitOfWork.GetRepository<Account>();
        var tokenRepo = unitOfWork.GetRepository<Domain.Entities.RefreshToken>();
        var roleRepo = unitOfWork.GetRepository<Domain.Entities.Role>();

        var existingUser = await userRepo.BuildQuery
            .Where(x => x.Email == request.Request.Email)
            .FirstOrDefaultAsync(cancellationToken);
        var role = await roleRepo.GetByIdAsync(request.Request.RoleId, cancellationToken);

        if (role == null)
        {
            return Response<AuthResponse>.Failure(Error.NotFound(message: "Quyền này không tồn tại"));
        }
        if (existingUser != null)
        {
            return Response<AuthResponse>.Failure(Error.Conflict(message: "Địa chỉ Email đã tồn tại"));
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

            // Generate tokens
            var accessToken = jwtProvider.Generate(newUser, role.Name);
            var refreshToken = jwtProvider.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                UserId = newUser.Id,
                Token = refreshToken,
                JwtId = Guid.NewGuid().ToString(),
                IsUsed = false,
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await tokenRepo.CreateAsync(refreshTokenEntity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = role.Name,
                Name = newUser.FullName ?? newUser.Username
            };

            return Response<AuthResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<AuthResponse>.Failure(Error.Conflict());
        }
    }
}