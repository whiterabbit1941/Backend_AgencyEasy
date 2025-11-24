using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignGBPService : IService<CampaignGBP, Guid>
    {
        Task<List<GbpLocation>> GetLocationList(Guid campaignId);

        Task<RootGbpData> GetGbpPerformanceData(Guid campaignId, string startDate, string endDate);
    }
}
