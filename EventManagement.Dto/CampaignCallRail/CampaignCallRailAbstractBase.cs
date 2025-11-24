using System;

namespace EventManagement.Dto
{
    public abstract class CampaignCallRailAbstractBase
    {
        /// <summary>
        /// CampaignCallRail Id.
        /// </summary>
        public Guid Id { get; set; }
        public string ApiKey { get; set; }
        public string AccoundId { get; set; }
        public string AccountName { get; set; }
        public int NumericId { get; set; }
        public Guid CampaignID { get; set; }

    }
   
}
