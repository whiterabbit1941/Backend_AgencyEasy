using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class ReportSetting : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
        public bool IsCoverPage { get; set; }        
        public string TableOfContent { get; set; }        
        public bool IsPageBreak { get; set; }
        public string Comments { get; set; } 
        public Guid?  CampaignId { get; set; }

        public Guid CompanyId { get; set; }
        public string HeaderSettings { get; set; }

        public string Html { get; set; }

        public string Frequency { get; set; }

        public string Images { get; set; }

        public string GoogleSheetSettings { get; set; }

        public string MailchimpSettings { get; set; }

        public string IndexSettings { get; set; }
    }
}
