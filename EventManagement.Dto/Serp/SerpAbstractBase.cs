using System;

namespace EventManagement.Dto
{
    public abstract class SerpAbstractBase
    {
        /// <summary>
        /// Serp Id.
        /// </summary>
        public Guid Id { get; set; }

        public Guid CampaignID { get; set; }

        public long Position { get; set; }

        public long Searches { get; set; }

        public string Location { get; set; }

        public string Keywords { get; set; }
        public string LocationName { get; set; }
    }
}
