using CoralTime.Common.Constants;
using CoralTime.DAL.Models;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoralTime.Services
{
    public class IdentityWithAdditionalClaimsProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public IdentityWithAdditionalClaimsProfileService(
            UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IConfiguration config)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _config = config;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject)?.Value;
            var user = await _userManager.FindByIdAsync(sub);

            var principal = await _claimsFactory.CreateAsync(user);

            var resultClaims = principal.Claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type))
                .GroupBy(x => x.Type + x.Value).Select(x => x.First())
                .Select(x => new Claim(type: x.Type, value: x.Value)).ToList();

            resultClaims.Add(new Claim(type: Constants.JwtRefreshTokenLifeTimeClaimType, value: _config["SlidingRefreshTokenLifetime"]));

            var userRole = user.Role;
            var isAdmin = userRole == Constants.ApplicationRoleAdmin;
            var policy = isAdmin ? Constants.AdminPolicies : Constants.UserPolicies;
            var jsonPolicy = JsonConvert.SerializeObject(policy);
            resultClaims.Add(new Claim(type: Constants.PoliciesClaimType, value: jsonPolicy));

            context.IssuedClaims = resultClaims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject)?.Value;
            var user = await _userManager.FindByIdAsync(sub);
            if (user != null)
            {
                if (user.IsActive)
                {
                    context.IsActive = user.IsActive;
                }
            }
        }
    }
}