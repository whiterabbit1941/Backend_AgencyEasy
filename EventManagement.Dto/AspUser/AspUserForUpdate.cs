using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    /// <summary>
    /// AspUser Update Model.
    /// </summary>
    public class AspUserForUpdate : AspUserAbstractBase
    {
        public string UserName { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool ShowDemoProject { get; set; }
        public DateTime Birthday { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }

        public string Address { get; set; }

    }
}
