using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignService : IService<Campaign, Guid>
    {
        List<CampaignDto> GetCampaignByUserId(string userId);

        Task<CampaignUserDto> AddUserToCampaign(string userId, string campaignId, string companyId);

        Task<string> GetUpdateDashboard();

        Task<List<Campaign>> UpdateDashboardData(List<Campaign> campaignRepo,DateTime startDate, DateTime endDate);

        Task UpdateDashboardTable(Guid id, int type);

        Task<CampaignIntegraionDto> GetCampaignIntegrationStatus(Guid campaignId);

        Task<List<Campaign>> GetDashboardData(List<Campaign> campaignRepo, string startDate, string endDate);

        Task<bool> UpdateDashboardDataAfterIntegration(Guid campaignId);

        Task<int> DeleteCampaignsFromAppsumo(List<Guid> ids);
    }
}
