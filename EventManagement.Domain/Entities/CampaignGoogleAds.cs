using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignGoogleAds : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string LoginCustomerID { get; set; }

        public string EmailId { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
    }
}
