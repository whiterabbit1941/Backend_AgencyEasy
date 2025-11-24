using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace EventManagement.Domain.Entities
{
    public class CampaignMailchimp : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string AccountName { get; set; }

        [Required]
        [MaxLength(250)]
        public string AccountId { get; set; }

        //Mailchimp Marketing access tokens do not expire, so you don't need to use a refresh_token .
        //The access token will remain valid unless the user revokes your application's permission to access their account.
        public string AccessToken { get; set; }
        public Guid CampaignID { get; set; }
        public string ApiEndpoint { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
    }
}
