using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IReportSettingService : IService<ReportSetting, Guid>
    {
        List<ReportSetting> GetReportByCampaign(string campaignId, Guid companyId);
    }
}
