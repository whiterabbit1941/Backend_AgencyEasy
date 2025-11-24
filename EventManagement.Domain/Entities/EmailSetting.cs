using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class EmailSetting : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }       
       
        public string EmailId { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }

    }
}
