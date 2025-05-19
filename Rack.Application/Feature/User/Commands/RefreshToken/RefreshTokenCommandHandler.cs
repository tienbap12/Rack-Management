using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication.Response;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.User.Commands.RefreshToken;

internal class RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtProvider jwtProvider)
    : ICommandHandler<RefreshTokenCommand, Response<TokenResponse>>
{
    public async Task<Response<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        //            await unitOfWork.BeginTransactionAsync(cancellationToken);
        var tokenRepo = unitOfWork.GetRepository<Domain.Entities.RefreshToken>();

        var existingToken = await tokenRepo.BuildQuery
            .Where(rt => rt.Token == request.RefreshToken)
            .Include(rt => rt.Account).ThenInclude(a => a.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken is null ||
            existingToken.IsUsed ||
            existingToken.IsRevoked ||
            existingToken.ExpiryDate < DateTime.UtcNow)
        {
            return Response<TokenResponse>.Failure(Error.Unauthorized(message: "Refresh token không hợp lệ hoặc đã hết hạn"));
        }

        var account = existingToken.Account;

        // Đánh dấu token cũ là đã sử dụng
        existingToken.IsUsed = true;
        existingToken.IsRevoked = true;

        // Tạo token mới
        var newAccessToken = jwtProvider.Generate(account, account.Role.Name);
        var newRefreshToken = jwtProvider.GenerateRefreshToken();

        // Lưu refresh token mới
        var newTokenEntity = new Domain.Entities.RefreshToken
        {
            UserId = account.Id,
            Token = newRefreshToken,
            JwtId = Guid.NewGuid().ToString(),
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        await tokenRepo.CreateAsync(newTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenResponse = new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };

        return Response<TokenResponse>.Success(tokenResponse);
    }
}