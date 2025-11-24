using System;
using System.ComponentModel.DataAnnotations;

namespace Marvin.IDP.Entities
{
    public abstract class AuditableEntity
    {
        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string UpdatedBy { get; set; }

        [MaxLength(500)]
        public string CompanyName { get; set; }

        public string Data { get; set; }

        public string EntityID { get; set; }

        [MaxLength(100)]
        public string EntityIndexedId { get; set; }
    }
}