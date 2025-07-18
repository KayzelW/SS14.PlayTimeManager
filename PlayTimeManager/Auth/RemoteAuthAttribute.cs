using System.Collections.Frozen;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace PlayTimeManager.Attributes;

public sealed class RemoteAuthAttribute(string type) : ActionFilterAttribute
{
    private bool _isLoaded = false;
    private HashSet<string>? _secTokens;

    const string Method = "Bearer";

    private void Load(IServiceProvider serviceProvider)
    {
        _isLoaded = true;
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        _secTokens = config.GetValue<HashSet<string>>("RemoteAuth:" + type)!;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;

        if (!_isLoaded)
            Load(context.HttpContext.RequestServices);

        if (_secTokens == null)
        {
            context.Result = Err("В конфиге отсутствует секция RemoteAuth:" + type, 401U);
            return;
        }

        var auth = request.Headers.Authorization;
        if (auth.Count != 1)
        {
            response.Headers.Append("WWW-Authenticate", new StringValues(Method));
            context.Result = Err("Не авторизован", 401U);
            return;
        }

        var authorization = auth.FirstOrDefault()?.Split(' ');
        if (authorization == null || authorization.Length != 2)
        {
            response.Headers.Append("WWW-Authenticate", new StringValues(Method));
            context.Result = Err("Не корректный запрос (Authorization)", 401U);
            return;
        }

        if (!string.Equals(authorization[0], Method, StringComparison.InvariantCultureIgnoreCase))
        {
            response.Headers.Append("WWW-Authenticate", new StringValues(Method));
            context.Result = Err("Не поддерживается тип авторизации", 401U);
            return;
        }

        if (!_secTokens.Contains(authorization[1]))
        {
            response.Headers.Append("WWW-Authenticate", new StringValues(Method));
            context.Result = Err("Не авторизован token", 401U);
            return;
        }

        context.HttpContext.User = new GenericPrincipal(new GenericIdentity(type, "RemoteAuth-" + type), new[]
        {
            "RemoteAuth",
            "RemoteAuth-" + type
        });
    }

    private IActionResult Err(string msg, float nerr = 500U)
    {
        return new BadRequestObjectResult(new { err = msg, code = nerr });
    }
}