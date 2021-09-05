using System;
using System.Collections.Generic;
using System.Text;
using LogProxy.Auth;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LogProxy.UnitTests
{
    public class BasicAuthenticatorTests
    {
        [Fact]
        public void Authenticate_NullHeaders_ArgumentNullException()
        {
            var authenticator = GetAuthenticator();

            var authResult = authenticator.Authenticate(null);
            
            Assert.False(authResult.Succeeded);
            Assert.NotNull(authResult.Failure);
        }
        
        [Fact]
        public void Authenticate_NoAuthHeader_AuthFail()
        {
            var authenticator = GetAuthenticator();

            var authResult = authenticator.Authenticate(new Dictionary<string, string>());

            Assert.False(authResult.Succeeded);
            Assert.NotNull(authResult.Failure);
        }
        
        [Theory]
        [InlineData("Basic")]
        [InlineData("BasicdXNlcjpwYXNzd29yZA==")]
        [InlineData("NotBasic dXNlcjpwYXNzd29yZA==")]
        [InlineData("Bearer dXNlcjpwYXNzd29yZA==")]
        public void Authenticate_HeaderWithInvalidValue_AuthFail(string authValue)
        {
            var authenticator = GetAuthenticator();
            var headers = new Dictionary<string, string> {{"Authorization", authValue}};

            var authResult = authenticator.Authenticate(headers);

            Assert.False(authResult.Succeeded);
            Assert.NotNull(authResult.Failure);
        }
        
        [Theory]
        [InlineData("userPassword")]
        [InlineData("user-Password")]
        [InlineData("user_password")]
        public void Authenticate_Base64EncodedIsNotUserColonPassword_AuthFail(string someStuff)
        {
            var authenticator = GetAuthenticator();
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(someStuff));
            var headers = new Dictionary<string, string> {{"Authorization", $"Basic {encoded}"}};

            var authResult = authenticator.Authenticate(headers);

            Assert.False(authResult.Succeeded);
            Assert.NotNull(authResult.Failure);
        }
        
        [Theory]
        [InlineData("user", "password")]
        [InlineData("i_dont", "care")]
        [InlineData("i", "know")]
        [InlineData("i_dont", "")]
        [InlineData("", "know")]
        [InlineData("", "")]
        public void Authenticate_InvalidUserPassword_AuthFail(string user, string password)
        {
            var authenticator = GetAuthenticator("i_dont", "know");
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
            var headers = new Dictionary<string, string> {{"Authorization", $"Basic {encoded}"}};

            var authResult = authenticator.Authenticate(headers);

            Assert.False(authResult.Succeeded);
            Assert.NotNull(authResult.Failure);
        }
        
        [Theory]
        [InlineData("user", "password")]
        [InlineData("i_dont", "care")]
        [InlineData("i", "know")]
        public void Authenticate_ValidUserPassword_AuthSuccess(string user, string password)
        {
            var authenticator = GetAuthenticator(user, password);
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
            var headers = new Dictionary<string, string> {{"Authorization", $"Basic {encoded}"}};

            var authResult = authenticator.Authenticate(headers);

            Assert.True(authResult.Succeeded);
            Assert.NotNull(authResult.Principal);
        }

        private IBasicAuthenticator GetAuthenticator(string user = "user", string password = "password")
        {
            var optionsMock = new Mock<IOptions<BasicAuthenticatorConfig>>();
            optionsMock.Setup(x => x.Value).Returns(new BasicAuthenticatorConfig
            {
                User = user,
                Password = password
            });
            return new BasicAuthenticator(optionsMock.Object);
        }
    }
}