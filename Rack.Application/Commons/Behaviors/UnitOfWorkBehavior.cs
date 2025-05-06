using MediatR;
using Microsoft.Extensions.Logging;
using Rack.Domain.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Commons.Behaviors{
    public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull{
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

            try
            {
                if (_unitOfWork.HasActiveTransaction)
                {
                    _logger.LogInformation("Committing transaction for {Request}", typeof(TRequest).Name);
                    await _unitOfWork.CommitAsync(cancellationToken);
                    _logger.LogInformation("Transaction committed successfully for {Request}", typeof(TRequest).Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while committing transaction for {Request}", typeof(TRequest).Name);
                if (_unitOfWork.HasActiveTransaction)
                {
                    _logger.LogInformation("Rolling back transaction for {Request}", typeof(TRequest).Name);
                    await _unitOfWork.RollBackAsync(cancellationToken);
                }
                throw;
            }

            return response;
        }
    }
}