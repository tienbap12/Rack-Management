using Microsoft.EntityFrameworkCore;
using Rack.Application.Commons.Abstractions;
using Rack.Application.Wrappers;
using Rack.Contracts.Authentication;
using Rack.Doamin.Commons.Primitives;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Commands.Login;

public class LoginCommandHandler(IUnitOfWork unitOfWork,IJwtProvider jwtProvider,
    IPasswordHashChecker passwordHashChecker) : ICommandHandler<LoginCommand, Response<AuthResponse>>{
    public async Task<Response<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var accountRepo = unitOfWork.GetRepository<Account>();
        var tokenRepo = unitOfWork.GetRepository<RefreshToken>();
        
        var user = await accountRepo.BuildQuery.Include(r => r.Roles).Where(x => x.Email == request.Email).FirstOrDefaultAsync(cancellationToken);
        
        if (user is null)
        {
            return Response<AuthResponse>.NotFoundUserName(request.Email);
        }
        var isValidPass = passwordHashChecker.HashesMatch(request.Password, user);
        if (!isValidPass)
        {
            return Response<AuthResponse>.Failure("Invalid Password");
        }
        
        var token = jwtProvider.Generate(new Account
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Password = user.Password,
            DoB = user.DoB,
            RoleId = user.RoleId,
            Phone = user.Phone,
            Roles = new Role
            {
                Id = user.RoleId,
                Name = user.Roles.Name
            }
        });

        var refreshToken = new RefreshToken()
        {
            UserId = user.Id,
            Token = token,
            JwtId = Guid.NewGuid().ToString(),
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddMonths(1) 
        };
        await tokenRepo.CreateAsync(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var result = new AuthResponse
        {
            token = token,
            Role = user.Roles.Name,
        };
        return Response<AuthResponse>.Success("Login successfully!!!", result);
    }
}