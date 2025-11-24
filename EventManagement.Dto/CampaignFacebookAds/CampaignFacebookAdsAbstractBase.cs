using System;

namespace EventManagement.Dto
{
    public abstract class CampaignFacebookAdsAbstractBase
    {
        /// <summary>
        /// CampaignFacebookAds Id.
        /// </summary>
        public Guid Id { get; set; }
        public string AdAccountName { get; set; }

        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        /// <summary>
        /// CampaignFacebookAds Name.
        /// </summary>

    }
}
