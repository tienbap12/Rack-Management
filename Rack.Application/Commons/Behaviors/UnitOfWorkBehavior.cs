using MediatR;
using Microsoft.Extensions.Logging;
using Rack.Domain.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Commons.Behaviors
{
    public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

        public UnitOfWorkBehavior(
            IUnitOfWork unitOfWork,
            ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();

            // Bỏ logic commit/rollback transaction thủ công để tránh lỗi với SqlServerRetryingExecutionStrategy
            // Nếu cần transaction, hãy dùng SaveChangesAsync hoặc execution strategy ở handler

            return response;
        }
    }
}