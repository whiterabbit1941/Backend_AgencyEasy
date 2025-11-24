using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignMicrosoftAdService : IService<CampaignMicrosoftAd, Guid>
    {
        Task<List<MsAdAccountListDto>> GetMsAdAccountList(Guid campaignId);

        Task<RootCampaignPerformace> GetCampaignPerformanceReport(Guid campaignId,string startDate,string endDate, long adCampaignId = 0);

        Task<RootAdGroupPerformance> GetAdGroupPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0);

        Task<RootKeywordPerformance> GetKeywordPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0);

        Task<RootConversionPerformance> GetConversionPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0);

        Task<List<MsAdCampaignList>> GetCampaignList(Guid campaignId);
    }
}
