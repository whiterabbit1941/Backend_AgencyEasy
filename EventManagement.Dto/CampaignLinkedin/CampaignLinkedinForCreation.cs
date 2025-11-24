using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class CampaignLinkedinForCreation : CampaignLinkedinAbstractBase
    {
        public Guid Id { get; set; }
        public string PageName { get; set; }
        public Guid CampaignID { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenExpiresIn { get; set; }

        public string OrganizationalEntity { get; set; }

        public string RefreshToken { get; set; }

        public string RefreshTokenExpiresIn { get; set; }

    }
}
