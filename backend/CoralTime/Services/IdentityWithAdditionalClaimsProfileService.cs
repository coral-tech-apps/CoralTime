using CoralTime.Common.Constants;
using CoralTime.DAL.Models;
using CoralTime.DAL.Repositories;
using CoralTime.DAL.Repositories.Member;
using CoralTime.ViewModels.Reports.Responce.DropDowns;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CoralTime.Services
{
    public class IdentityWithAdditionalClaimsProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly ILogger<IdentityWithAdditionalClaimsProfileService> _logger;

        public IdentityWithAdditionalClaimsProfileService(
            UserManager<ApplicationUser> userManager,
            UnitOfWork uow,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IConfiguration config,
            ILogger<IdentityWithAdditionalClaimsProfileService> logger
            )
        {
            _userManager = userManager;
            _uow = uow;
            _claimsFactory = claimsFactory;
            _config = config;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject)?.Value;
            var user = await _userManager.FindByIdAsync(sub);
            var member = _uow.MemberRepository.LinkedCacheGetByUserId(user.Id);
            //var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation($"Data for subject: {sub}");
            _logger.LogInformation($"User: {user}");

            var principal = await _claimsFactory.CreateAsync(user);

            var resultClaims = principal.Claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type))
                .Select(x=> new Claim(type: x.Type, value: x.Value))
                .Distinct().ToList();
            
            resultClaims.Add(new Claim(type: Constants.JwtIsManagerClaimType, value: user.IsManager.ToString().ToLower()));
            resultClaims.Add(new Claim(type: Constants.JwtRefreshTokenLifeTimeClaimType, value: _config["SlidingRefreshTokenLifetime"]));

            resultClaims.Add(new Claim(JwtClaimTypes.Audience, Constants.Authorization.WebApiScope));
            resultClaims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
            //TODO: add aud
            //resultClaims.Add(new Claim(JwtClaimTypes.Audience, "WebAPI"));
            resultClaims.Add(new Claim("MemberId", member.Id.ToString()));
            //resultClaims.Add(new Claim("audience", "WebAPI"));

            var roles = await _userManager.GetRolesAsync(user);
            foreach(var role in roles)
            {
               resultClaims.Add(new Claim("role", role));
            }

            foreach(var claim in resultClaims)
            {
                _logger.LogInformation($"Added claim: {claim.Type} = {claim.Value}");
            }

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