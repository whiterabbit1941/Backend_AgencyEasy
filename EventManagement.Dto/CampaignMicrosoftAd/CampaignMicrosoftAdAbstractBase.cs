using System;

namespace EventManagement.Dto
{
    public abstract class CampaignMicrosoftAdAbstractBase
    {
        /// <summary>
        /// CampaignMicrosoftAd Id.
        /// </summary>
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public string AccountId { get; set; }
        public Guid CampaignID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int AccessExpire { get; set; }

    }
}
