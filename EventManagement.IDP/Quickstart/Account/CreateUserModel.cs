using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    public class CreateUserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Database { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; }

        public string ID { get; set; }
        public string Birthday { get; set; }
        public string Birthplace { get; set; }
        public string Gender { get; set; }
        public string Occupation { get; set; }
        public string PhoneNumber { get; set; }
        public string LivesIn { get; set; }
        public string AdminName { get; set; }
        public string ReturnURL { get; set; }
        public CreateUserModel()
        {
            IsActive = false;
            FirstName = "";
            LastName = "";
            Database = "";
            Email = "";
            ID = "";
            Password = "";
            Birthday = "";
            Birthplace = "";
            Gender = "";
            Occupation = "";
            PhoneNumber = "";
            LivesIn = "";
            ImageUrl = "";
            AdminName = "";
            ReturnURL = "";
        }
    }
}
