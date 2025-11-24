using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CompanyPlan : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }


        public virtual Company Company { get; set; }


        [Required]
        [MaxLength(250)]
        [ForeignKey("Company")]
        public Guid CompanyId { get; set; }


        public virtual DefaultPlan DefaultPlan { get; set; }

        [Required]
        [ForeignKey("DefaultPlan")]
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
