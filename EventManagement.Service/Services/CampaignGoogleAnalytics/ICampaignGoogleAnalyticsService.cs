using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface ICampaignGoogleAnalyticsService : IService<CampaignGoogleAnalytics, Guid>
    {
        Task<bool> UpdateRefreshTokenAndEmail(CampaignGoogleAnalyticsForCreation campaigngoogleanalytics, string companyId);

        string GoogleAnalyticsSetup(string type, string source);

        Task<GaToken> GetAccessTokenUsingCode(string code);

        Task<GaToken> GetAccessTokenUsingCodeForPopup(string code);

        Task<RootObjectGoogleAnayltics> GetAnalyticsProfileIds(Guid campaignId);

        Task<RootObjectOfGoogleEmail> GetEmailAddress(Guid campaignId, string type);

        Task<GoogleAnalyticsResponseDto> GetCampaignGaDataById(Guid campaignId, string startDate, string endDate);

        Task<Ga4Details> GetCampaignGa4DataById(Guid campaignId, string startDate, string endDate);

        Task<string> GetLighthouseDataByStrategy(Guid campaignId, string strategy);

        Task<Ga4RootList> GetAnalytics4ProfileIds(Guid campaignId);

        Task<bool> IsPropertiesExists(string access_token);
    }
}
