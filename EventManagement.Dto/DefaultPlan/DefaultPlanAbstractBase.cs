using System;

namespace EventManagement.Dto
{
    public abstract class DefaultPlanAbstractBase
    {
        /// <summary>
        /// DefaultPlan Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// DefaultPlan Name.
        /// </summary>
        public string Name { get; set; }

        public decimal Cost { get; set; }

        public int MaxProjects { get; set; }

        public int MaxTeamUsers { get; set; }

        public int MaxClientUsers { get; set; }

        public int MaxKeywordsPerProject { get; set; }
    }
}
