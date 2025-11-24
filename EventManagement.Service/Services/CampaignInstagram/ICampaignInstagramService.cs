using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignInstagramService : IService<CampaignInstagram, Guid>
    {
        Task<RootObjectInstaData> GetInstagramPageLists(List<InstaList> fbpages, Guid campaignId);
        Task<RootObjectInstaData> GetInstaIds(List<FacebookList> facebookPage, Guid campaignId);
        Task<RootObjectFBData> GetFaceBookPageList(Guid campaignId);
        Task<InstagramReportsData> GetInstagramReportDataById(Guid campaignId, string fromDate, string toDate);
        bool IsPermissionGranted(string accessToken);
    }
}
