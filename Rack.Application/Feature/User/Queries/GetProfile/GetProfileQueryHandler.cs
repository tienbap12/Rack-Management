using MediatR;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Queries.GetProfile;

public class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, ProfileResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Response<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var account = await accountRepo.BuildQuery
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Id.ToString() == request.UserId, cancellationToken);

        if (account == null)
        {
            return Response<ProfileResponse>.Failure(Error.NotFound(message: "User not found"));
        }

        var profile = new ProfileResponse
        {
            Id = account.Id,
            Username = account.Username,
            FullName = account.FullName,
            DoB = account.DoB,
            Phone = account.Phone,
            Email = account.Email,
            Role = account.Role.Name,
            CreatedOn = account.CreatedOn,
            LastModifiedOn = account.LastModifiedOn
        };

        return Response<ProfileResponse>.Success(profile);
    }
}