using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Rack.Domain.Commons.Primitives;
using System.Reflection;

namespace Rack.Application.Commons.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Thực hiện validation
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                // Tạo Error object từ các lỗi validation
                var error = CreateValidationError(failures);

                // Tạo response failure phù hợp với kiểu TResponse
                return CreateErrorResponse<TResponse>(error);
            }

            return await next();
        }

        private static Error CreateValidationError(IEnumerable<ValidationFailure> failures)
        {
            // Chuyển đổi FluentValidation errors sang Error system
            var errorDetails = failures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToList();

            return Error.Validation(
                message: $"Validation failed: {string.Join("; ", errorDetails)}"
            );
        }

        private static TResponse CreateErrorResponse<TResponse>(Error error)
        {
            // Xử lý cho các loại response khác nhau
            if (typeof(TResponse) == typeof(Response))
            {
                return (TResponse)(object)Response.Failure(error);
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Response<>))
            {
                var genericType = typeof(TResponse).GetGenericArguments()[0];
                var responseType = typeof(Response<>).MakeGenericType(genericType);

                return (TResponse)responseType
                    .GetMethod("Failure", BindingFlags.Public | BindingFlags.Static)!
                    .Invoke(null, [error])!;
            }

            throw new InvalidOperationException(
                $"Unsupported response type: {typeof(TResponse).Name}"
            );
        }
    }
}