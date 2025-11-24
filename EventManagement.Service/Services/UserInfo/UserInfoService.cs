using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using EventManagement.Domain;
using System.Collections.Generic;
using System.Security.Claims;

namespace EventManagement.Service
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string UserId { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string Role { get; set; }
        public string SelectedCompanyId { get; set; }
        public Boolean SuperAdmin { get; set; }
        public string CustomDomain { get; set; }

        public UserInfoService(IHttpContextAccessor httpContextAccessor)
        {
            // service is scoped, created once for each request => we only need
            // to fetch the info in the constructor
            _httpContextAccessor = httpContextAccessor 
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            var currentContext = _httpContextAccessor.HttpContext;
            if (currentContext == null || !currentContext.User.Identity.IsAuthenticated)
            {
                if (currentContext != null)
                {
                    string[] companyId = currentContext.Request.Headers.GetCommaSeparatedValues("SelectedCompanyId");
                    if (companyId.Count() > 0)
                    {
                        SelectedCompanyId = companyId[0];
                    }
                }
                return;
            }
            string[] companyIds = currentContext.Request.Headers.GetCommaSeparatedValues("SelectedCompanyId");
            if (companyIds.Count() > 0)
            {
                SelectedCompanyId = companyIds[0];
            }
            string[] customDomains = currentContext.Request.Headers.GetCommaSeparatedValues("CustomDomain");
            if (customDomains.Count() > 0)
            {
                CustomDomain = customDomains[0];
            }
            var identity = currentContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                // or

                UserId = (identity.FindFirst("sub")?.Value) ?? "n/a";
                FirstName = (identity.FindFirst("given_name")?.Value) ?? "n/a";
                LastName = (identity.FindFirst("given_lname")?.Value) ?? "n/a";
                Role = (identity.FindFirst("role")?.Value) ?? "n/a";
                var isSuperAdmin = (identity.FindFirst("super_admin")?.Value) ?? "false";
                SuperAdmin = Convert.ToBoolean(isSuperAdmin);                
            }
        }
    }
}
