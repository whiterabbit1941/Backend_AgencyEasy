using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class AppsumoPlan : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public string AppsumoPlanId { get; set; }
        public decimal Cost { get; set; }
        public int MaxProjects { get; set; }
        public int MaxKeywordsPerProject { get; set; }
        public bool WhitelabelSupport { get; set; }
        public int MaxTeamUsers { get; set; }
        public int MaxClientUsers { get; set; }
    }
}
