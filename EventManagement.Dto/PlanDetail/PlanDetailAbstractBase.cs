using EventManagement.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    public abstract class PlanDetailAbstractBase
    {
        /// <summary>
        /// PlanDetail Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// PlanDetail Name.
        /// </summary>
        public Guid PlanId { get; set; }
        public Guid FeatureID { get; set; }

        public bool Visibility { get; set; }

        public List<Feature> FeatureList { get; set; }

        public virtual DefaultPlan DefaultPlan { get; set; }

        public virtual Feature FeatureDto { get; set; }


    }
}
