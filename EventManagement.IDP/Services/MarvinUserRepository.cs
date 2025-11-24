using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Marvin.IDP.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marvin.IDP.Services
{
    public class MarvinUserRepository : IMarvinUserRepository
    {
        MarvinUserContext _context;

        public MarvinUserRepository(MarvinUserContext context)
        {
            _context = context;
        }

        public bool AreUserCredentialsValid(string username, string password)
        {
            // get the user
            var key = Encoding.UTF8.GetBytes("abcdefg");
            var hash = new HMACSHA256(key);
            var message = Encoding.UTF8.GetBytes(password);
            byte[] signature = hash.ComputeHash(message);
            StringBuilder hexDigest = new StringBuilder();
            foreach (byte b in signature)
                hexDigest.Append(String.Format("{0:x2}", b));

            var user = GetUserByUsername(username);
            if (user == null)
            {
                return false;
            }

            return (user.PasswordHash == password && !string.IsNullOrWhiteSpace(password));
        }

        //public User GetUserByEmail(string email)
        //{
        //    return _context.Users.FirstOrDefault(u => u.Claims.Any(c => c.ClaimType == "email" && c.ClaimValue == email));
        //}

        //public User GetUserByProvider(string loginProvider, string providerKey)
        //{
        //    return _context.Users
        //        .FirstOrDefault(u => 
        //            u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
        //}

        public IdentityUser GetUserBySubjectId(string subjectId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == subjectId);
        }
        public IdentityUser GetUserWithClaimsBySubjectId(string subjectId)
        {
            return _context.Users.Include("UserClaims").FirstOrDefault(u => u.Id == subjectId);
        }

        public IEnumerable<IdentityUser> GetUserWithClaims()
        {
            return _context.Users.Include("UserClaims");
        }

        public IdentityUser GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.NormalizedUserName == username);
        }

        public IEnumerable<IdentityUserClaim<string>> GetUserClaimsBySubjectId(string subjectId)
        {
            // get user with claims
            //var user = _context.Users.Include("UserClaims").FirstOrDefault(u => u.Id == subjectId);
            //if (user == null)
            //{
            //    return new List<IdentityUserClaim>();
            //}
            return _context.UserClaims.Where(x=>x.UserId == subjectId).ToList();
        }

        //public IEnumerable<UserLogin> GetUserLoginsBySubjectId(string subjectId)
        //{
        //    var user = _context.Users.Include("Logins").FirstOrDefault(u => u.SubjectId == subjectId);
        //    if (user == null)
        //    {
        //        return new List<UserLogin>();
        //    }
        //    return user.Logins.ToList();
        //}

        public bool IsUserActive(string subjectId)
        {
            return true;
            //var user = GetUserBySubjectId(subjectId);
            //return user.PhoneNumberConfirmed;
         }

        public void AddUser(IdentityUser user)
        {
            _context.Users.Add(user);
        }

        public void UpdateUser(IdentityUser user)
        {
            _context.Users.Update(user);
        }

        public void AddUserLogin(string subjectId, string loginProvider, string providerKey)
        {
            //var user = GetUserBySubjectId(subjectId);
            //if (user == null)
            //{
            //    throw new ArgumentException("User with given subjectId not found.", subjectId);
            //}

            //user.Logins.Add(new UserLogin()
            //{
            //    SubjectId = subjectId,
            //    LoginProvider = loginProvider,
            //    ProviderKey = providerKey
            //});
        }

        public void AddUserClaim(string subjectId, string claimType, string claimValue)
        {          
            var user = GetUserBySubjectId(subjectId);
            if (user == null)
            {
                throw new ArgumentException("User with given subjectId not found.", subjectId);
            }

            //user.Claims.Add(new UserClaim(claimType, claimValue));         
            var claim = new IdentityUserClaim<string>();
            claim.ClaimType = claimType;
            claim.ClaimValue = claimValue;
            claim.UserId = subjectId;
            _context.UserClaims.Add(claim);
        }
        public void UpdateUserClaim(string subjectId, string claimType, string claimValue)
        {
            var user = GetUserBySubjectId(subjectId);
            if (user == null)
            {
                throw new ArgumentException("User with given subjectId not found.", subjectId);
            }

            //user.Claims.Add(new UserClaim(claimType, claimValue));         
            var claim = _context.UserClaims.Where(x => x.ClaimType == claimType).FirstOrDefault();
            if(claim != null)
            {
                claim.ClaimType = claimType;
                claim.ClaimValue = claimValue;
                claim.UserId = subjectId;
                _context.UserClaims.Update(claim);
            }
            else
            {
                AddUserClaim(subjectId, claimType, claimValue);
            }
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public IdentityUser GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public IdentityUser GetUserByProvider(string loginProvider, string providerKey)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserLogin> GetUserLoginsBySubjectId(string subjectId)
        {
            throw new NotImplementedException();
        }
    }
}
