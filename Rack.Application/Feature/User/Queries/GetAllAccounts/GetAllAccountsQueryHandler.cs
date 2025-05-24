// using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.User.Queries.GetAllAccounts;

public class GetAllAccountsQueryHandler : IQueryHandler<GetAllAccountsQuery, List<AccountResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAccountsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Response<List<AccountResponse>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        // Store result in IQueryable<Account> to avoid type conversion issues
        IQueryable<Account> query = accountRepo.BuildQuery.Include(a => a.Role);

        // Filter deleted accounts unless explicitly included
        if (!request.IncludeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        var accounts = await query.ToListAsync(cancellationToken);

        var accountResponses = accounts.Select(account => new AccountResponse
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
        }).ToList();

        return Response<List<AccountResponse>>.Success(accountResponses);
    }
}