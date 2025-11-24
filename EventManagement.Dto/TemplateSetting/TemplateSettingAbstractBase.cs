using System;

namespace EventManagement.Dto
{
    public abstract class TemplateSettingAbstractBase
    {
        /// <summary>
        /// TemplateSetting Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// TemplateSetting Name.
        /// </summary>
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
