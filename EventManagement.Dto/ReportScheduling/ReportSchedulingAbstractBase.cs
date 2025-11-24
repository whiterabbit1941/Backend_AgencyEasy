using EventManagement.Domain.Entities;
using EventManagement.Utility.Enums;
using System;

namespace EventManagement.Dto
{
    public abstract class ReportSchedulingAbstractBase
    {
        /// <summary>
        /// ReportScheduling Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ReportScheduling Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ScheduleDateAndTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EmaildIds { get; set; }

       
        /// <summary>
        /// Subject of report
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Report Setting
        /// </summary>
        public virtual ReportSetting ReportSetting { get; set; }

          /// <summary>
          /// Report Id
          /// </summary>
        public Guid ReportId { get; set; }

        public ReportScheduleType Scheduled { get; set; }
        public int Day { get; set; }
        public bool Status { get; set; }
        public string HtmlHeader { get; set; }
        public string HtmlFooter { get; set; }

    }


}
