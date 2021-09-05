using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LogProxy.Auth
{
    public interface IBasicAuthenticator
    {
        public AuthenticateResult Authenticate(Dictionary<string, string> requestHeaders);
    }

    public class BasicAuthenticatorConfig
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
    
    public class BasicAuthenticator : IBasicAuthenticator
    {
        private readonly BasicAuthenticatorConfig _config;

        public BasicAuthenticator(IOptions<BasicAuthenticatorConfig> options)
        {
            _config = options.Value;
        }

        public AuthenticateResult Authenticate(Dictionary<string, string> requestHeaders)
        {
            if (requestHeaders == null || !requestHeaders.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("no auth header");

            var authValue = requestHeaders["Authorization"];
            if (!authValue.StartsWith("Basic "))
                return AuthenticateResult.Fail("wrong auth method");

            var (user, password) = GetUserAndPasswordFromBasicAuthValue(authValue);
            if (user == null || password == null)
                return AuthenticateResult.Fail("failed to extract user/password");
            
            if (!IsAuthorized(user, password))
                return AuthenticateResult.Fail("user/password invalid");
            
            return AuthenticateResult.Success(GetAuthenticationTicket(user));
        }

        private (string, string) GetUserAndPasswordFromBasicAuthValue(string authValue)
        {
            var authValueSplit = authValue.Trim().Split(" ");
            if (authValueSplit.Length != 2)
                return (null, null);

            var base64EncodedUserAndPassword = authValueSplit[1];
            var decodedUserAndPassword =
                Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedUserAndPassword));
            var userPasswordSplit = decodedUserAndPassword.Split(":");
            if (userPasswordSplit.Length != 2)
                return (null, null);
            
            return (userPasswordSplit[0], userPasswordSplit[1]);
        }

        private bool IsAuthorized(string user, string password)
        {
            return user == _config.User && password == _config.Password;
        }
        
        private AuthenticationTicket GetAuthenticationTicket(string user)
        {
            var identity = new ClaimsIdentity(user);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, "BasicAuthentication");
        }
    }
}