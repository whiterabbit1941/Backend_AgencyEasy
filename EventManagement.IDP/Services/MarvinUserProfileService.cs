using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.AspNetIdentity;
using Microsoft.Extensions.Logging;
using Marvin.IDP.Entities;
using IdentityModel;

namespace Marvin.IDP.Services
{
    public class MarvinUserProfileService : ProfileService<User>
    {
        /// <summary>
        /// The claims factory.
        /// </summary>
        protected readonly IUserClaimsPrincipalFactory<User> ClaimsFactory;

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger<ProfileService<User>> Logger;

        /// <summary>
        /// The user manager.
        /// </summary>
        protected readonly UserManager<User> userManager;

        protected readonly IApplicationAccountRepository _applicationAccountRepository;

        public MarvinUserProfileService(UserManager<User> _userManager, IApplicationAccountRepository applicationAccountRepository,
            IUserClaimsPrincipalFactory<User> claimsFactory) : base(_userManager, claimsFactory)
        {
            userManager = _userManager;
          //  userManager.PasswordHasher = new MyPasswordHasher();
            ClaimsFactory = claimsFactory;
            _applicationAccountRepository = applicationAccountRepository;
        }



        public override async Task  GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await userManager.FindByIdAsync(context.Subject.GetSubjectId());

            var claims = await getClaims(user);

            //if (!context.AllClaimsRequested)
            //{
            //    claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
            //}

            context.IssuedClaims = claims;
            //await base.GetProfileDataAsync(context);
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Subject == null) throw new ArgumentNullException(nameof(context.Subject));

            context.IsActive = false;

            var subject = context.Subject;
            var user = await userManager.FindByIdAsync(context.Subject.GetSubjectId());

            if (user != null)
            {
                var security_stamp_changed = false;

                if (userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = (
                        from claim in subject.Claims
                        where claim.Type == "security_stamp"
                        select claim.Value
                        ).SingleOrDefault();

                    if (security_stamp != null)
                    {
                        var latest_security_stamp = await userManager.GetSecurityStampAsync(user);
                        security_stamp_changed = security_stamp != latest_security_stamp;
                    }
                }

                context.IsActive =
                    !security_stamp_changed &&
                    !await userManager.IsLockedOutAsync(user);
            }
        }

        private async Task<List<Claim>> getClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, await userManager.GetUserIdAsync(user)),
                new Claim(JwtClaimTypes.Name, await userManager.GetUserNameAsync(user))
            };

            if (userManager.SupportsUserEmail)
            {
                var email = await userManager.GetEmailAsync(user);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified,
                            await userManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (userManager.SupportsUserPhoneNumber)
            {
                var phoneNumber = await userManager.GetPhoneNumberAsync(user);
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified,
                            await userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (userManager.SupportsUserClaim)
            {
                claims.AddRange(await userManager.GetClaimsAsync(user));
            }

            if (userManager.SupportsUserRole)
            {
                var roles = await userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
            }

            // Add databases
            if (user.Database != null)
            {
                claims.Add(new Claim("Database", user.Database));
            }
            if (user.LivesIn != null)
            {
                claims.Add(new Claim("livesIn", user.LivesIn));
            }
            if (user.Occupation != null)
            {
                claims.Add(new Claim("phoneNumber", user.PhoneNumber));
            }
            if (user.Occupation != null)
            {
                claims.Add(new Claim("occupation", user.Occupation));
            }
            if (user.Gender != null)
            {
                claims.Add(new Claim("gender", user.Gender));
            }
            if (user.Birthplace != null)
            {
                claims.Add(new Claim("birthplace", user.Birthplace));
            }
            if (user.Birthday != null)
            {
                claims.Add(new Claim("birthday", user.Birthday.ToString("MM/dd/yyyy")));
            }
            //// Added given FName and LName from DB instead of the claims table. 
            if (user.FName != null)
            {
                claims.Add(new Claim("given_name", user.FName));
            }
            if (user.LName != null)
            {
                claims.Add(new Claim("given_lname", user.LName));
            }
            claims.Add(new Claim("ShowDemoProject", user.ShowDemoProject.ToString(), ClaimValueTypes.Boolean));
            //if (user.ImageUrl != null)
            //{
            //    claims.Add(new Claim("image_url", user.ImageUrl));
            //}
            //var accountList = _applicationAccountRepository.GetUserAccounts().Where(x => x.UserId == user.Id).ToList();
            //List<Guid> accountIDList = new List<Guid>();
            //foreach (var account in accountList)
            //{
            //    claims.Add(new Claim("account_"+account.AccountId.ToString(), account.PrimaryUser.ToString()));
            //    accountIDList.Add(account.AccountId);
            //}


            //var clientList = _applicationAccountRepository.GetClientUsers().Where(x => x.UserID == user.Id && accountIDList.Contains(x.AccountId))
            //    .Select(o => new
            //    {
            //        DBName = o.Client.DBName,
            //        Role = o.Role
            //    }).ToList();

            //foreach(var client in clientList)
            //{
            //    claims.Add(new Claim("client_"+client.DBName, client.Role));
            //}

            var key = await userManager.GetAuthenticatorKeyAsync(user);
           
            claims.Add(new Claim("IsAuthenticatorConfigured", string.IsNullOrEmpty(key) ? "false" : "true", ClaimValueTypes.Boolean));
            claims.Add(new Claim("TwoFactorEnabled", await userManager.GetTwoFactorEnabledAsync(user) ? "true" : "false", ClaimValueTypes.Boolean));
            return claims;
        }
    }
}
