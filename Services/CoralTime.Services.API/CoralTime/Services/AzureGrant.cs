using CoralTime.Common.Constants;
using CoralTime.DAL.Models;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CoralTime.Services.API.Services
{
    public class AzureGrant : IExtensionGrantValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AzureGrant> _logger;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;

        public AzureGrant(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger<AzureGrant> logger,
            IConfigurationManager<OpenIdConnectConfiguration> configManager)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
            _configManager = configManager;
        }

        public string GrantType => Constants.Authorization.CoralTimeAzure.GrantType;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get(Constants.Authorization.CoralTimeAzure.UserTokenHeader);

            if (string.IsNullOrEmpty(userToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Token is empty");
                return;
            }

            try
            {
                var token = await ValidateTokenAsync(userToken);
                if (token == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid token");
                    return;
                }

                var userName = token.Claims.FirstOrDefault(m => m.Type == Constants.Authorization.CoralTimeAzure.UserNameClaim)?.Value;

                var user = await _userManager.FindByEmailAsync(userName);

                if (user != null && ((user?.IsActive) ?? false))
                {
                    context.Result = new GrantValidationResult(
                        subject: user.Id,
                        authenticationMethod: Constants.Authorization.CoralTimeAzure.AuthenticationMethod,
                        claims: GetUserClaims(user));
                    return;
                }

                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, null);
            }
        }

        private async Task<JwtSecurityToken> ValidateTokenAsync(string jwtToken)
        {
            try
            {
                var openIdConfig = await _configManager.GetConfigurationAsync(CancellationToken.None);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = openIdConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _config["Authentication:AzureAd:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(jwtToken, tokenValidationParameters, out var validatedToken);
                return validatedToken as JwtSecurityToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Token validation failed");
                return null;
            }
        }

        private static IEnumerable<Claim> GetUserClaims(ApplicationUser user)
        {
            return new[]
            {
                new Claim(type: "user_id", value: user.Id ?? ""),
                new Claim(type: JwtClaimTypes.Email, value: user.Email ?? "")
            };
        }
    }
}
