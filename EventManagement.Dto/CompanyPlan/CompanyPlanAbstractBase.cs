using System;

namespace EventManagement.Dto
{
    public abstract class CompanyPlanAbstractBase
    {
        /// <summary>
        /// CompanyPlan Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// CompanyPlan Name.
        /// </summary>
        public Guid CompanyId { get; set; }
        public Guid DefaultPlanId { get; set; }

        public DateTime ExpiredOn { get; set; }

        public string PaymentProfileId { get; set; }

        public bool Active { get; set; }

        public int MaxProjects { get; set; }

        public int MaxTeamUsers { get; set; }

        public int MaxClientUsers { get; set; }

        public int MaxKeywordsPerProject { get; set; }

        public bool IsDowngradeAppsumo { get; set; }

    }
}
