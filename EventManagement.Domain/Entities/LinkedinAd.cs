using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class LinkedinAd : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string PageName { get; set; }
        public Guid CampaignID { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenExpiresIn { get; set; }

        public string OrganizationalEntity { get; set; }

        public string RefreshToken { get; set; }

        public string Currency { get; set; }

        public string RefreshTokenExpiresIn { get; set; }

        [ForeignKey("CampaignID")]

        public virtual Campaign Campaign { get; set; }
    }
}
