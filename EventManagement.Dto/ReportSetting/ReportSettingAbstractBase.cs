using System;

namespace EventManagement.Dto
{
    public abstract class ReportSettingAbstractBase
    {
        /// <summary>
        /// ReportSetting Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ReportSetting Name.
        /// </summary>
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
        public bool IsCoverPage { get; set; }
        public string TableOfContent { get; set; }
        public bool IsPageBreak { get; set; }
        public string Comments { get; set; }
        public Guid? CampaignId { get; set; }

        public Guid CompanyId { get; set; }

        public string HeaderSettings { get; set; }

        public string Html { get; set; }

        public string Frequency { get; set; }
        public string Images { get; set; }

        public string Googlesheetsettings { get; set; }

        public string MailchimpSettings { get; set; }

        public string IndexSettings { get; set; }        
    }
}
