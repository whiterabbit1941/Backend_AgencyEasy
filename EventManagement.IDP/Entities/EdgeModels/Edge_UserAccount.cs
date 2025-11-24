
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marvin.IDP.Entities
{
    public class Edge_UserAccount : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Edge_Account Account { get; set; }

        [Required]
        [ForeignKey("Edge_Account")]
        public Guid AccountId { get; set; }

        public virtual User User { get; set; }

        [Required]
        [ForeignKey("AspNetUsers")]
        public string UserId { get; set; }

        public bool PrimaryUser { get; set; }
    }
}
