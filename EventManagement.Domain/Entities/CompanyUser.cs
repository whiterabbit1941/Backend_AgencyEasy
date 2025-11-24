using FinanaceManagement.API.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CompanyUser : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Company Company { get; set; }

        [Required]
        [ForeignKey("Company")]
        public Guid CompanyId { get; set; }

        public string UserId { get; set; }

        public string Role { get; set; }
    }
}
