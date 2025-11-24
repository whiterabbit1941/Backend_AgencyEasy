using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignFacebookAds : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
            
        public string AdAccountName { get; set; }
        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        [ForeignKey("CampaignID")]

        public virtual Campaign Campaign { get; set; }
    }
}
