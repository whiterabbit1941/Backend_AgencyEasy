using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class Serp : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
       
        public Guid CampaignID { get; set; }

        public long Position { get; set; }

        public long LocalPackCount { get; set; }

        public long Searches { get; set; }

        public string Location { get; set; }
        public string LocationName { get; set; }

        public string Keywords { get; set; }

        public string LambdaLogger { get; set; }

        public string BusinessName { get; set; }

        public bool LocalPacksStatus { get; set; }

        public Guid? TaskId { get; set; }

        public bool IsWebhookRecieved { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
    }
}
