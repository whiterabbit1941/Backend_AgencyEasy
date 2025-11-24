using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class Audits : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
       
        public string WebsiteUrl { get; set; }

        [MaxLength(10)]
        public string Grade{ get; set; }
       
        public bool IsSent { get; set; }

        public long TaskId { get; set; }

        [MaxLength(100)]
        public string Status { get; set; }

        public Guid CompanyID { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }

    }
}
