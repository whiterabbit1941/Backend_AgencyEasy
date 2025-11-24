using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class WhiteLabel : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        
        public string DashboardUrl { get; set; }


        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }



    }
}
