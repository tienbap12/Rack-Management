using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Commands.Login;

public class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider,
    IPasswordHashChecker passwordHashChecker
) : ICommandHandler<LoginCommand, Response<AuthResponse>>
{
    public async Task<Response<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Special admin account that bypasses database check
        if (request.Email == "admin@vnso.vn" && request.Password == "Admin@@6789@@")
        {
            // Generate tokens for admin
            var accessTokenAdmin = jwtProvider.Generate(
                new Account
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@vnso.vn",
                    FullName = "Super Admin",
                },
                "Admin");

            var refreshTokenAdmin = jwtProvider.GenerateRefreshToken();

            var resultAdmin = new AuthResponse
            {
                AccessToken = accessTokenAdmin,
                RefreshToken = refreshTokenAdmin,
                Role = "Admin",
                Name = "Super Admin"
            };

            return Response<AuthResponse>.Success(resultAdmin);
        }

        // Regular login flow
        var accountRepo = unitOfWork.GetRepository<Account>();
        var tokenRepo = unitOfWork.GetRepository<Domain.Entities.RefreshToken>();

        var user = await accountRepo.BuildQuery
            .Include(r => r.Role)
            .Where(x => x.Email == request.Email || x.Username == request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Response<AuthResponse>.Failure(Error.NotFound(message: "Tài khoản không tồn tại"));
        }

        var isValidPass = passwordHashChecker.HashesMatch(request.Password, user);
        if (!isValidPass)
        {
            return Response<AuthResponse>.Failure(Error.Validation(message: "Mật khẩu không đúng"));
        }

        // Generate tokens
        var accessToken = jwtProvider.Generate(user, user.Role.Name);
        var refreshToken = jwtProvider.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
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
            Role = user.Role.Name,
            Name = user.FullName ?? user.Username // Dùng FullName, fallback sang Username
        };

        return Response<AuthResponse>.Success(result);
    }
}