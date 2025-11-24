using Marvin.IDP.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP.Services
{
    public interface IMarvinUserRepository
    {
        IdentityUser GetUserByUsername(string username);
        IdentityUser GetUserBySubjectId(string subjectId);
        IdentityUser GetUserByEmail(string email);
        IdentityUser GetUserByProvider(string loginProvider, string providerKey);
        IdentityUser GetUserWithClaimsBySubjectId(string subjectId);
        IEnumerable<IdentityUser> GetUserWithClaims();
        IEnumerable<UserLogin> GetUserLoginsBySubjectId(string subjectId);
        IEnumerable<IdentityUserClaim<string>> GetUserClaimsBySubjectId(string subjectId);
        bool AreUserCredentialsValid(string username, string password);
        bool IsUserActive(string subjectId);
        void AddUser(IdentityUser user);
        void AddUserLogin(string subjectId, string loginProvider, string providerKey);
        void AddUserClaim(string subjectId, string claimType, string claimValue);
        void UpdateUserClaim(string subjectId, string claimType, string claimValue);

        void UpdateUser(IdentityUser user);
        bool Save();
    }
}
