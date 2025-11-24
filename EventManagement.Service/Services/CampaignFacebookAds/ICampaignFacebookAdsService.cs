using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface ICampaignFacebookAdsService : IService<CampaignFacebookAds, Guid>
    {
        Task<RootObjectFBAdsData> GetFbAllAdsAccountDetails(Guid campaignId);

        Task<FacebookGetData> GetFbAdsData(Guid campignId, int frequency,DateTime startDate,DateTime endDate );

        bool IsPermissionGranted(string accessToken);


    }
}
