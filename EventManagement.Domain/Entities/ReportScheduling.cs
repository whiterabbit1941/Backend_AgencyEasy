using EventManagement.Utility.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class ReportScheduling : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime ScheduleDateAndTime{ get; set; }
        public ReportScheduleType Scheduled { get; set; }
        public int Day { get; set; }
        public bool Status { get; set; }
        public string EmaildIds { get; set; }

        [MaxLength(998)]
        public string Subject { get; set; }
        public string HtmlHeader { get; set; }
        public string HtmlFooter { get; set; }
        public virtual ReportSetting ReportSetting { get; set; }

        [Required]
        [ForeignKey("ReportSetting")]
        public Guid ReportId { get; set; }
    }
}
