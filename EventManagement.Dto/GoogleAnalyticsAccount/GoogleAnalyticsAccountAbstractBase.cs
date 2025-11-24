using System;

namespace EventManagement.Dto
{
    public abstract class GoogleAnalyticsAccountAbstractBase
    {
        /// <summary>
        /// GoogleAnalyticsAccount Id.
        /// </summary>
        public Guid Id { get; set; }
        public Guid GoogleAccountSetupID { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string WebsiteUrl { get; set; }
        public string PropertyID { get; set; }
        public string ViewName { get; set; }
        public string ViewID { get; set; }
        public string CampaignID { get; set; }
        public bool Active { get; set; }
    }
}
