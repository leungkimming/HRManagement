﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace P6.StoryTest.Support
{
    public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
    {
        public const string UserId = "UserId";

        public const string AuthenticationScheme = "Test";
        private readonly string _defaultUserId;

        public TestAuthHandler(
            IOptionsMonitor<TestAuthHandlerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _defaultUserId = options.CurrentValue.DefaultUserId;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            List<Claim> claims;

            if (Context.Request.Headers.TryGetValue(UserId, out var userId))
            {
                claims = new List<Claim> { new Claim(ClaimTypes.Name, userId) };
            }
            else
            {
                claims = new List<Claim> { new Claim(ClaimTypes.Name, "Test user") };
            }

            // Extract User ID from the request headers if it exists,
            // otherwise use the default User ID from the options.
            if (Context.Request.Headers.TryGetValue("Authorization", out var auth))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, auth[0]));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, _defaultUserId));
            }

            // TODO: Add as many claims as you need here
            claims.Add(new Claim(ClaimTypes.Role, "AA01"));
            claims.Add(new Claim(ClaimTypes.Role, "AB01"));
            claims.Add(new Claim(ClaimTypes.Role, "AC01"));

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
