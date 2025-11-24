using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// AspUser Model
    /// </summary>
    public class AspUserDto : AspUserAbstractBase
    {
        public int AccessFailedCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string Email { get; set; }
        
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool ShowDemoProject { get; set; }
        public string UserName { get; set; }
        public string Database { get; set; }
        public bool IsActive { get; set; }
        public DateTime Birthday { get; set; }
        public string Birthplace { get; set; }
        public string Gender { get; set; }
        public string LivesIn { get; set; }
        public string Occupation { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string ImageUrl { get; set; }
        public bool Role { get; set; }
        public Guid CompanyID { get; set; }
        public string Address { get; set; }

        public Guid RowGuid { get; set; }

        public DateTime CreatedOn { get; set; }

        public AspUserDto()
        {
            AccessFailedCount = 0;
            EmailConfirmed = false;
            LockoutEnabled = true;
            PhoneNumberConfirmed = false;
            TwoFactorEnabled = false;
            ShowDemoProject = true;
            Birthday = new DateTime(1971, 1, 1);           
        }

       
    }
    public class CompanysUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string CompanyRole { get; set; }

        public Guid CompanyId { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool ShowDemoProject { get; set; }

    }

}
 