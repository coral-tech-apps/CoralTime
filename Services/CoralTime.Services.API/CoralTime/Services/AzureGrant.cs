using CoralTime.Common.Constants;
using CoralTime.DAL.Models;
using CoralTime.ViewModels.Azure;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoralTime.Services.API.Services
{
    public class AzureGrant : IExtensionGrantValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AzureGrant> _logger;
        private readonly IMemoryCache _memoryCache;

        public AzureGrant(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger<AzureGrant> logger,
            IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
            _memoryCache = memoryCache;
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
                var certificates = await GetCertificateKeysAsync();
                var tokenToCheck = new JwtSecurityToken(jwtToken);
                var x5t = tokenToCheck.Header.X5t;

                string Normalize(string s) => s?.Replace('-', '+').Replace('_', '/');
                var normalizedX5t = Normalize(x5t);

                var matchingKey = certificates.Keys.FirstOrDefault(k => Normalize(k.X5t) == normalizedX5t);
                if (matchingKey?.X5c?.FirstOrDefault() is not string x5cBase64 || string.IsNullOrWhiteSpace(x5cBase64))
                {
                    _logger.LogError("Matching certificate not found for x5t: {x5t}", x5t);
                    return null;
                }

                var rawCertData = Convert.FromBase64String(x5cBase64);
                var cert = X509CertificateLoader.LoadCertificate(rawCertData);
                var key = new X509SecurityKey(cert);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Authentication:AzureAd:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = true,
                    ValidAudience = _config["Authentication:AzureAd:Audience"]
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


    private async Task<CertificateKeys> GetCertificateKeysAsync()
        {
            var certificateKeys = _memoryCache.TryGetValue(Constants.CertificateKeys, out CertificateKeys certificates);
            var certificateKeysTime = _memoryCache.TryGetValue(Constants.CertificateKeysTime, out DateTime certificatesTime);

           if (!certificateKeys || !certificateKeysTime || DateTime.Now.Subtract(certificatesTime).TotalHours >= 24)
            {
                var url = _config["Authentication:AzureAd:CertificatesUrl"];
                var client = new HttpClient();
                var json = await client.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                certificates = JsonSerializer.Deserialize<CertificateKeys>(json, options);
                
                _memoryCache.Set(Constants.CertificateKeysTime, DateTime.Now);
                _memoryCache.Set(Constants.CertificateKeys, certificates);
            }

            return certificates;
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