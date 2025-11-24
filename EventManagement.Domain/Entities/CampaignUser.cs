using FinanaceManagement.API.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignUser : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Campaign Campaign { get; set; }
        [Required]
        [ForeignKey("Campaign")]
        public Guid CampaignId { get; set; }

        public string UserId { get; set; }

        public Guid CompanyId { get; set; }
    }
}
