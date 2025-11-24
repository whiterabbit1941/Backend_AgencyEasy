using System;

namespace EventManagement.Dto
{
    public abstract class CampaignGoogleAdsAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string CustomerId { get; set; }
        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }

        public string LoginCustomerID { get; set; }

    }
}
