using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Migrations;
using CampaignFacebookAds = EventManagement.Domain.Entities.CampaignFacebookAds;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IReportSchedulingService : IService<ReportScheduling, Guid>
    {
        Task<bool> EmailReportSchedule();

        Task<string> GetAccessTokenUsingRefreshToken(string refreshToken);

        PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate);

        Task<GaIntegrationData> PrepareGaOrganicTrafficReports(string htmlString, string access_token, string startDate, string endDate, string url, Guid campaignId, string gaProfileId, PreviousDate previousDate, bool fromReportScheduling, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader,int pageNumber);

        Task<GscIntegrationData> PrepareGoogleSearchConsole(string startDate, string endDate, string urlOrName, string accessToken, PreviousDate previousDate,List<string> subtypes);

        Task<GscChartResponse> PrepareGSCReportsByPostWithDate(string preparedUrl, string startDate, string endDate, string accessToken);

        Task<GscData> PrepareGSCReportsByPost(string preparedUrl, string startDate, string endDate, string accessToken);

        Task<GoogleAnalyticsResponseDto> PrepareGAReportsByPost(string accessToken, string startDate, string endDate, string profileId);

        Task<GA4Root> PrepareGa4OrganicTrafficReportsByGet(string accessToken, string fromDate, string toDate, string profileId);

        Task<string> PreparePageSpeedLighthouseByStrategy(string url, string strategy);
        
        //Task<string> GenerateReportAndDownload(GenerateReportDto generateReportDto);

        //Task<string> PrepareFacebookAdsReport(string htmlString, Guid campaignId, DateTime startDate, DateTime endDate, string accessToken, CampaignFacebookAds facebookAdsSetup, PreviousDate previousDate, string campaignLogo, string companyLogo, string headerText,int type);

        Task<FacebookAdsCampaignData> PrepareFacebookAdsCampaignsView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList);

        Task<FacebookAdsCampaignData> PrepareFacebookAdsAdsGroupView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList);

        Task<FacebookAdsCampaignData> PrepareFacebookAdsCopiesView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList);

        Task<List<FacebookReportDates>> PrepareDateListForFacebook(DateTime startDate, DateTime endDate);

        Task<Ga4Details> PrepareGa4ChartData(GA4Root gA4Root, string startDate, string endDate);

        Task<string> PreparePdfReport(GenerateReportDto generateReportDto);

        Task<bool> SendReportInEmailFronEnd(ShareReportPdfEmail shareReportPdfEmail);

        Task<List<EcomPurchase>> PrepareGa4EcomReportsByGet(string accessToken, string fromDate, string toDate, string profileId);

        Task<Ga4PurchaseJourney> PrepareGa4PurchaseJourneyReports(string accessToken, string fromDate, string toDate, string profileId);

    }
}
