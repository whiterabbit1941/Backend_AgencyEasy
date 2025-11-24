using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class PlanDetail : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual DefaultPlan DefaultPlan { get; set; }
    
        [Required]
        [ForeignKey("DefaultPlan")]
        public Guid DefaultPlanId { get; set; }

        public virtual Feature Feature { get; set; }

        [Required]
        [ForeignKey("Feature")] 
        public Guid FeatureID { get; set; }

        public bool Visibility { get; set; }
    }
}
