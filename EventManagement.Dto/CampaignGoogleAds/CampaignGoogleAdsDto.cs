using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignGoogleAds Model
    /// </summary>
    public class CampaignGoogleAdsDto : CampaignGoogleAdsAbstractBase
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string LoginCustomerID { get; set; }

    }
    public class GoogleAdsCustomerDto
    {
        public long CustomerId { get; set; }

        public string Name { get; set; }

        public string LoginCustomerId { get; set; }

    }

    public class GoogleAdsCampaignReport
    {
        public string Date { get; set; } 
        public string Name { get; set; }

        public string AdGroupId { get; set; }

        public string CampaignId { get; set; }

        public string AdId { get; set; }

        public long ViewThroughConversions { get; set; }

        public double Avg_CPC { get; set; }
        public long Clicks { get; set; }

        public double ConversationRate { get; set; }

        public double Conversation { get; set; }

        public double Cost{ get; set; }

        public string CostPerConversions { get; set; }

        public long Impressions { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public string Interaction { get; set; }

        public string Currency { get; set; }

    }
}
 