using MediatR;
using Rack.Domain.Commons.Primitives;
using System.Reflection;

namespace Rack.Application.Commons.Behaviors;

public class ResponseWrapperBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(); // Pass through normally
        }
        catch (Exception ex)
        {
            var error = Error.Failure(Domain.Enum.ErrorCode.GeneralInternalServerError, ex.Message);

            if (typeof(TResponse) == typeof(Response))
                return (TResponse)(object)Response.Failure(error);

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Response<>))
            {
                var genericType = typeof(TResponse).GetGenericArguments()[0];
                var responseType = typeof(Response<>).MakeGenericType(genericType);

                var failureMethod = responseType
                    .GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, new[] { typeof(Error) });

                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[] { error });
                    return (TResponse)result!;
                }
            }

            throw; // Rethrow if cannot wrap
        }
    }
}