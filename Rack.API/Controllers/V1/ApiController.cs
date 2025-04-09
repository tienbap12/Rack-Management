using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rack.Domain.Commons.Primitives;

namespace Rack.API.Controllers.V1
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
            .GetService<IMediator>();

        protected IActionResult ToActionResult(Response response)
        {
            if (!response.IsSuccess)
            {
                return new ObjectResult(new
                {
                    response.Message,
                    response.Error,
                    response.StatusCode
                })
                {
                    StatusCode = (int)response.StatusCode
                };
            }

            return Ok(new
            {
                response.Message,
                response.StatusCode
            });
        }

        protected IActionResult ToActionResult<T>(Response<T> response)
        {
            if (!response.IsSuccess)
            {
                return new ObjectResult(new
                {
                    response.Message,
                    response.Error,
                    response.StatusCode
                })
                {
                    StatusCode = (int)response.StatusCode
                };
            }

            return Ok(new
            {
                response.Message,
                response.StatusCode,
                response.Data
            });
        }
    }
}