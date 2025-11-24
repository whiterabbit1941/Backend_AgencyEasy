using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class DefaultPlan : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        public decimal Cost { get; set; }

        public int MaxProjects { get; set; }

        public int MaxTeamUsers { get; set; }

        public int MaxClientUsers { get; set; }

        public int MaxKeywordsPerProject { get; set; }

        public bool IsVisible { get; set; }
    }
}
