using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class GoogleAnalyticsAccount : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        
      
        public string CampaignID { get; set; }


        public Guid GoogleAccountSetupID { get; set; }

        
        [MaxLength(250)]
        public string AccountID { get; set; }

        
        [MaxLength(250)]
        public string AccountName { get; set; }

              
        public string WebsiteUrl { get; set; }

        
        [MaxLength(250)]
        public string PropertyID { get; set; }

        
        [MaxLength(250)]
        public string ViewName { get; set; }

     
        [MaxLength(250)]
        public string ViewID { get; set; }

        public bool Active { get; set; }


        [ForeignKey("GoogleAccountSetupID")]
        public virtual GoogleAccountSetup GoogleAccountSetup { get; set; }

    }
}
