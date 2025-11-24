using System;

namespace EventManagement.Dto
{
    public abstract class CampaignWooCommerceAbstractBase
    {
        public Guid Id { get; set; }
        public Guid CampaignID { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string ShopUrl { get; set; }

    }
}
