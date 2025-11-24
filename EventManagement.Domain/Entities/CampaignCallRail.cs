using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignCallRail : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string ApiKey { get; set; }
        public string AccoundId { get; set; }
        public string AccountName { get; set; }
        public int NumericId { get; set; }
        public Guid CampaignID { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
    }
}
