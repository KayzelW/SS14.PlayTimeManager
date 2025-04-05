using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PlayTimeManager.Attributes;

public class AccessSection
{
    public string[] rw { get; set; } = [];
    public string[] ro { get; set; } = [];
}

public class TokenSchemeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private AccessSection options;

    public TokenSchemeHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock, AccessSection accessSection) : base(options, logger, encoder, clock)
    {
        this.options = accessSection;
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = $"Bearer, charset=\"UTF-8\"";
        await base.HandleChallengeAsync(properties);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
// #if DEBUG
//             {
//                 var cl = new List<Claim>();
//                 cl.Add(new Claim(ClaimsIdentity.DefaultIssuer, "WebServer"));
//                 cl.Add(new Claim(ClaimTypes.Name, "DEBUG"));
//                 cl.Add(new Claim(ClaimTypes.Role, "ro"));
//                 cl.Add(new Claim(ClaimTypes.Role, "rw"));
//                 return AuthenticateResult.Success(
//                     new AuthenticationTicket(
//                         new ClaimsPrincipal(new List<ClaimsIdentity>() { new ClaimsIdentity(cl) }), "Basic")
//                 );
//             }
// #endif

        if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"],
                out AuthenticationHeaderValue headerValue))
        {
            //Invalid Authorization header
            return AuthenticateResult.NoResult();
        }

        if (!"Bearer".Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        if (options.ro.Contains(headerValue.Parameter, StringComparer.CurrentCultureIgnoreCase))
        {
            var cl = new List<Claim>();
            cl.Add(new Claim(ClaimsIdentity.DefaultIssuer, "WebServer"));
            cl.Add(new Claim(ClaimTypes.Name, headerValue.Parameter));
            cl.Add(new Claim(ClaimTypes.Role, "ro"));
            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(new List<ClaimsIdentity>() { new ClaimsIdentity(cl) }), "Basic")
            );
        }
        
        if (options.rw.Contains(headerValue.Parameter, StringComparer.CurrentCultureIgnoreCase))
        {
            var cl = new List<Claim>();
            cl.Add(new Claim(ClaimsIdentity.DefaultIssuer, "WebServer"));
            cl.Add(new Claim(ClaimTypes.Name, headerValue.Parameter));
            cl.Add(new Claim(ClaimTypes.Role, "ro"));
            cl.Add(new Claim(ClaimTypes.Role, "rw"));
            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(new List<ClaimsIdentity>() { new ClaimsIdentity(cl) }), "Basic")
            );
        }

        return AuthenticateResult.Fail("Permission denied");
    }
}