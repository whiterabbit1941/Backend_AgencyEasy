using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// Campaign Model
    /// </summary>
    public class CampaignDto : CampaignAbstractBase
    {

    }

    public class CampaignIntegraionDto
    {
        public bool GoogleSearchConsole { get; set; }
        public bool GoogleAnalytics { get; set; }
        public bool GoogleAnalytics4 { get; set; }
        public bool GoogleAds { get; set; }
        public bool Facebook { get; set; }
        public bool FacebookAds { get; set; }
        public bool Instagram { get; set; }
        public bool LinkedIn { get; set; }
        public bool Keyword { get; set; }
    }

    public class UserCampaignAccessDto
    {
        public Guid UserId { get; set; }
        public Guid CampaignId { get; set; }
        public String CampaignName { get; set; }
        public string CampaignType { get; set; }
        public bool IsAccess { get; set; }
    }

}
