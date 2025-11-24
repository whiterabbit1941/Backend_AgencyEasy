using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class TemplateSetting : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
        public bool IsCoverPage { get; set; }
        public string TableOfContent { get; set; }
        public bool IsPageBreak { get; set; }
        public string Comments { get; set; }
        public Guid CompanyId { get; set; }
        public string HeaderSettings { get; set; }
        public string Html { get; set; }
        public string Frequency { get; set; }
        public string Images { get; set; }
    }
}
