using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogProxy.Auth
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IBasicAuthenticator _authenticator;

        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IBasicAuthenticator authenticator) : base(options, logger, encoder,
            clock)
        {
            _authenticator = authenticator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Response.Headers.Add("WWW-Authenticate", "Basic");
            var requestHeaders = Request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
            var authenticationResult = _authenticator.Authenticate(requestHeaders);
            return await Task.FromResult(authenticationResult);
        }
    }
}