using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using static EventManagement.Dto.CampaignCallRailDto;

namespace EventManagement.Service
{
    public interface ICampaignCallRailService : IService<CampaignCallRail, Guid>
    {
        Task<CampaignCallRailDto> VaidateApiKeyAndSetup(string apiKey, Guid campaignId);
        Task<CallRailReportData> GetCallRailReport(CallReportDTO callReportDTO);

        Task<CallResponse> GetCallRailTableReport(Guid campaignId, string startDate, string endDate, int pageNumber);

        Task<CallResponse> GetAllCallRailCallsForPdf(Guid campaignId, string startDate, string endDate);

        Task<AccountResponse> GetAccountList(Guid campaignId);

        Task<Recording> GetRecording(Guid campaignId, string url);
    }
}
