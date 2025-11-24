using System;

namespace EventManagement.Dto
{
    public abstract class CampaignAbstractBase
    {
        /// <summary>
        /// Campaign Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Campaign Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Campaign WebUrl.
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// Campaign MoreTraffic.
        /// </summary>
        public bool MoreTraffic { get; set; }

        /// <summary>
        /// Campaign Sales.
        /// </summary>
        public bool Sales { get; set; }

        /// <summary>
        /// Campaign LeadGeneration.
        /// </summary>
        public bool LeadGeneration { get; set; }

        /// <summary>
        /// CompanyID
        /// </summary>
        public Guid CompanyID { get; set; }
        public string CampaignType { get; set; }
        public string UserId { get; set; }

        public string Ranking { get; set; }
        public string Traffic { get; set; }
        public string TrafficGa4 { get; set; }

        public string Conversions { get; set; }
        public string Gsc { get; set; }

        public string ExceptionDashboardLambda { get; set; }
        public DateTime LastUpdateDashboardDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
