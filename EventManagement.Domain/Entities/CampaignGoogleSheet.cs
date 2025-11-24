using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignGoogleSheet : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CampaignID { get; set; }
        public string AccountId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
        public string Settings { get; set; }
    }
}
