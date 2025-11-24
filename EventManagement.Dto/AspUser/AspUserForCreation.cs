using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class AspUserForCreation : AspUserAbstractBase
    {
        public string UserName { get; set; }

        public string Password { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public bool ShowDemoProject { get; set; } = true;
        public DateTime Birthday { get; set; }

        public Guid CompanyID { get; set; }
        public string CName { get; set; }
        public string Companytype { get; set; }
        public string CompanyRole { get; set; }

        public string CompanyImageUrl { get; set; }
        public bool Role { get; set; }
        public Guid[] CampaignID { get; set; }

        public string CustomDomain { get; set; }

        public string SubDomain { get; set; }

        public string Address { get; set; }

        public string NewPlanName { get; set; }

        public string NewPlanPaymentId { get; set; }

        public Guid RowGuid { get; set; }

       
        public AspUserForCreation()
        {
            Id = Guid.NewGuid().ToString();
            AccessFailedCount = 0;
            EmailConfirmed = false;
            LockoutEnabled = true;
            PhoneNumberConfirmed = false;
            TwoFactorEnabled = false;
            ShowDemoProject = true;
            Birthday = new DateTime(1971, 1, 1);
        }
    }

    public class AppsumoTokenResponseDto
    {
        public string access { get; set; }
    }
    public class AppsumoNotificationResponseDto
    {
        public string message { get; set; }
        public string redirect_url { get; set; }
    }
}
