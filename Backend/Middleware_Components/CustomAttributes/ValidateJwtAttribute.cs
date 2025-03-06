using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CourierAPI.Controllers.CustomAttributes
{
    public class ValidateJwtAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var jwtService = context.HttpContext.RequestServices.GetService<IJwtService>();
            var bearerToken = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (jwtService == null || string.IsNullOrEmpty(bearerToken))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var validation = await jwtService.AccessTokenValidation(bearerToken);
            if (validation.TokenHasError())
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
