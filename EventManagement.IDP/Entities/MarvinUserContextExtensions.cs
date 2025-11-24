using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.IDP.Entities
{
    public static class MarvinUserContextExtensions
    {
        public static void EnsureSeedDataForContext(this MarvinUserContext context)
        {
            // Add 2 demo users if there aren't any users yet
            if (context.Users.Any())
            {
                return;
            }

            var key = Encoding.UTF8.GetBytes("abcdefg");
            var hash = new HMACSHA256(key);
            var message = Encoding.UTF8.GetBytes("Vadera@2019");
            byte[] signature = hash.ComputeHash(message);
            StringBuilder hexDigest = new StringBuilder();
            foreach (byte b in signature)
                hexDigest.Append(String.Format("{0:x2}", b));


            // init users
            var users = new List<IdentityUser>()
            {
                new IdentityUser()
                {
                    Id = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb",
                    UserName = "Kevin",
                    NormalizedUserName = "kevin",
                    PasswordHash = hexDigest.ToString(),

                },
                new IdentityUser()
                {
                    Id = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d",
                    UserName = "Sven",
                    NormalizedUserName = "sven",
                    PasswordHash = hexDigest.ToString(),
                   
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        public static void EnsureSeedDataForContext(this ApplicationDbContext context)
        {
            // Add 2 demo users if there aren't any users yet
            if (context.Users.Any())
            {
                return;
            }

            HashAlgorithm algorithm = SHA256.Create();

            System.Security.Cryptography.MD5 hs = System.Security.Cryptography.MD5.Create();
            byte[] db = hs.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password"));
            string result = Convert.ToBase64String(db);

            // init users
            var users = new List<User>()
            {
                new User()
                {
                    Id = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb",
                    UserName = "Kevin",
                    NormalizedUserName = "kevin",
                    SecurityStamp = "user",
                    PasswordHash = result,
                    IsActive = true,
                    AccessFailedCount = 0,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                  
                },
                new User()
                {
                    Id = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d",
                    UserName = "Sven",
                    SecurityStamp = "user",
                    NormalizedUserName = "sven",
                    PasswordHash =result,
                    IsActive = true,
                    AccessFailedCount = 0,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                  
                }
            };

            context.Users.AddRange(users);

            var claim = new IdentityUserClaim<string>();
            //claim.Id = 1;
            claim.ClaimType = "given_name";
            claim.ClaimValue = "Kevin";
            claim.UserId = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb";

            context.UserClaims.Add(claim);


            claim = new IdentityUserClaim<string>();
            //claim.Id = 2;
            claim.ClaimType = "family_name";
            claim.ClaimValue = "Dockx";
            claim.UserId = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb";

            context.UserClaims.Add(claim);


            claim = new IdentityUserClaim<string>();
           // claim.Id = 3;
            claim.ClaimType = "role";
            claim.ClaimValue = "Administrator";
            claim.UserId = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb";

            context.UserClaims.Add(claim);


            claim = new IdentityUserClaim<string>();
           //claim.Id = 4;
            claim.ClaimType = "given_name";
            claim.ClaimValue = "Sven";
            claim.UserId = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d";

            context.UserClaims.Add(claim);


            claim = new IdentityUserClaim<string>();
            //claim.Id = 5;
            claim.ClaimType = "family_name";
            claim.ClaimValue = "Vercauteren";
            claim.UserId = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d";

            context.UserClaims.Add(claim);


            claim = new IdentityUserClaim<string>();
            //claim.Id = 6;
            claim.ClaimType = "role";
            claim.ClaimValue = "Tour Manager";
            claim.UserId = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d";

            context.UserClaims.Add(claim);

            context.SaveChanges();
        }
    }
}
