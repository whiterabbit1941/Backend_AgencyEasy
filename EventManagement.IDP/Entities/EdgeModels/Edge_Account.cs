
using Marvin.IDP.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marvin.IDP.Entities
{
    public class Edge_Account : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        public string BillingDetail { get; set; }

        public virtual User PrincipleOwner { get; set; }

        [Required]
        [ForeignKey("AspNetUsers")]
        public string PrincipleOwnerId { get; set; }
    }
}
