using System;

namespace EventManagement.Dto
{
    public abstract class AppsumoPlanAbstractBase
    {
        /// <summary>
        /// AppsumoPlan Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// AppsumoPlan Name.
        /// </summary>
        public string Name { get; set; }
        public string AppsumoPlanId { get; set; }
        public decimal Cost { get; set; }
        public int MaxProjects { get; set; }
        public int MaxKeywordsPerProject { get; set; }
        public bool WhitelabelSupport { get; set; }
        public int MaxTeamUsers { get; set; }
        public int MaxClientUsers { get; set; }

    }
}
