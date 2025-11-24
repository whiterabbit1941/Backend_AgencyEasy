using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// ReportSetting Model
    /// </summary>
    public class ReportSettingDto : ReportSettingAbstractBase
    {
        public virtual CampaignDto Campaign { get; set; }
    }
    public class ReportByCampaignDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; }
        public List<ReportSettingDto> ReportList { get; set; }
        public ReportByCampaignDto()
        {
            ReportList = new List<ReportSettingDto>();
        }
    }

}
 