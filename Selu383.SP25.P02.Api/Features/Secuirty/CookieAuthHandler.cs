using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Selu383.SP25.P02.Api.Security
{
    public class CookieAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public CookieAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1) Look for a cookie named "AuthCookie"
            var cookie = Request.Cookies["AuthCookie"];
            if (string.IsNullOrEmpty(cookie))
            {
                // No cookie => not authenticated => 401 if [Authorize] is used
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Format: "userId;username;role1,role2"
            var parts = cookie.Split(';');
            if (parts.Length != 3)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid cookie format"));
            }

            var userIdString = parts[0];
            var username = parts[1];
            var rolesCsv = parts[2];

            if (!int.TryParse(userIdString, out var userId))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid user id"));
            }

            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            // Add roles
            foreach (var role in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
