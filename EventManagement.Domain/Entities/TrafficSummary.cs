using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class TrafficSummary : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int AvragePosition { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public Guid CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }
    }
}
