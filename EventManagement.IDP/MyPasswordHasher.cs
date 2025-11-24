using Marvin.IDP.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP
{
    // This class is no longer used. This we used to store the naked password (without hashing) 
    // Now on we will be using the default impelementation of the asp.net identity and will be storing the 
    // hashed password in db.
    public class MyPasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password)
        {
            return password;
        }
        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword.Equals(providedPassword))
                return PasswordVerificationResult.Success;
            else return PasswordVerificationResult.Failed;
        }
    }
}
