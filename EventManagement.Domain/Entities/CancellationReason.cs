using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CancellationReason : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        [ForeignKey("Company")]
        public Guid CompanyId { get; set; }

        public virtual Company Company { get; set; }

        public string Reason { get; set; }

        public string OtherSolution { get; set; }

        public string Rating { get; set; }

        public string Feedback { get; set; }


    }
}
