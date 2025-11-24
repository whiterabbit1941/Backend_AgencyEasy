using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignGSCService : IService<CampaignGSC, Guid>
    {
        Task<GscData> GetCampaignGscDataById(Guid campaignId, string startDate, string endDate);
        Task<GscChartResponse> GetCampaignGscChartDataWithDateById(Guid campaignId, string startDate, string endDate);
        Task<bool> UpdateRefreshTokenAndEmail(CampaignGSCForCreation campaigngsc, string companyId);

        Task<GaToken> RefreshGoogleGSCToken(string accessToken);

        Task<RootObjectOfGSCList> GetGSCList(Guid campaignId);

        Task<bool> IsPropertiesExists(string access_token);
    }
}
