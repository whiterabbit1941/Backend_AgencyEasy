using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ILinkedinAdService : IService<LinkedinAd, Guid>
    {
        Task<LinkedinAdRoot> GetLinkedInPages(Guid campaignId);

        Task<AnalyticsRoot> GetLinkedinAdsAnalytics(string campaignId, string type, string startTime, string endTime);

        Task<DempgraphicRoot> GetLinkedinAdsDemographic(string campaignId, string type, string startTime, string endTime);

        List<LinkedinAdDto> GetCampaignLinkedinByCampaignId(string campaignId);

        Task<AnalyticsRoot> GetPreparedLinkedinAdData(string campaignId, string type, string startDate, string endDate);
    }
}
