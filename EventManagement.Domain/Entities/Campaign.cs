using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class Campaign : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public string WebUrl { get; set; }
        public Boolean MoreTraffic { get; set; }
        public Boolean Sales { get; set; }
        public Boolean LeadGeneration { get; set; }
        public Guid CompanyID { get; set; }
        public string CampaignType { get; set; }
        public string Ranking { get; set; }
        public string Traffic { get; set; }
        public string TrafficGa4 { get; set; }
        public string Conversions { get; set; }
        public string Gsc { get; set; }
        public string ExceptionDashboardLambda { get; set; }
        public DateTime LastUpdateDashboardDate { get; set; }
        public DateTime LastUpdateSerpDate { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
    }
}
