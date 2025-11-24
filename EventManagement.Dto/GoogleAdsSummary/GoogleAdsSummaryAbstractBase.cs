using System;

namespace EventManagement.Dto
{
    public abstract class GoogleAdsSummaryAbstractBase
    {
        public Guid Id { get; set; }
        public int AvragePosition { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public Guid CampaignId { get; set; }

    }
}
