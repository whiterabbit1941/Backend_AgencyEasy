using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CompanyPlan Model
    /// </summary>
    public class CompanyPlanDto : CompanyPlanAbstractBase
    {

    }

    public class CompanyPlanDetailDto
    {
        public Guid CompanyId { get; set; }

        public string SessionId { get; set; }

    }
    public class AppsumoPlanDetailDto
    {
        public Guid Id { get; set; }
        public Guid DefaultPlanId { get; set; }
        public string AppsumoPlanId { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public int MaxProjects { get; set; }
        public int MaxKeywordsPerProject { get; set; }
        public bool WhitelabelSupport { get; set; }
        public int MaxTeamUsers { get; set; }
        public int MaxClientUsers { get; set; }
        public List<string> FeatureList { get; set; }

    }

    public class CompanyTransactionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid DefaultPlanId { get; set; }
        public String DefaultPlanName { get; set; }
        public decimal DefaultPlanCost { get; set; }
        public string Type { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpiredOn { get; set; }
        public string PaymentProfileId { get; set; }
        public bool AppsumoPlan { get; set; }
        public bool Active { get; set; }
        public int MaxProjects { get; set; }
        public int MaxTeamUsers { get; set; }
        public int MaxClientUsers { get; set; }
        public int MaxKeywordsPerProject { get; set; }
    }
}
