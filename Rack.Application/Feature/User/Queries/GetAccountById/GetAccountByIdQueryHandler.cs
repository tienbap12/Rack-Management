using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Queries.GetAccountById;

public class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, AccountResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Response<AccountResponse>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var account = await accountRepo.BuildQuery
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            return Response<AccountResponse>.Failure(Error.NotFound(message: "Tài khoản không tồn tại"));
        }

        var response = new AccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            FullName = account.FullName,
            DoB = account.DoB,
            Phone = account.Phone,
            Email = account.Email,
            RoleName = account.Role.Name,
            RoleId = account.RoleId,
            CreatedOn = account.CreatedOn,
            LastModifiedOn = account.LastModifiedOn,
            IsDeleted = account.IsDeleted
        };

        return Response<AccountResponse>.Success(response);
    }

}