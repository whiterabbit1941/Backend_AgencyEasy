using System;

namespace EventManagement.Dto
{
    public abstract class CampaignGSCAbstractBase
    {
        public Guid Id { get; set; }
        public string UrlOrName { get; set; }

        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }


    }
}
