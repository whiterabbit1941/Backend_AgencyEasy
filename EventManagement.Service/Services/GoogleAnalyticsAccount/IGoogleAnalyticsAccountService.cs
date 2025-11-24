using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EventManagement.Service
{
    public interface IGoogleAnalyticsAccountService : IService<GoogleAnalyticsAccount, Guid>
    {
        /// <summary>
        /// Get GA setups by campaignID
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>List of GoogleAnalyticsAccountDto</returns>
        List<GoogleAnalyticsAccountDto> GetGaAccountByCampaignID(string campaignId);

        /// <summary>
        /// InActive All Ga Analytics While Select New 
        /// </summary>
        /// <param name="id">Google analytics Account Id</param>
        /// <returns>Int</returns>
        Task<int> InActiveAllGaAnalytics(Guid id);

        /// <summary>
        /// Get Ga Analytics Reports By CampaignId
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>GaReportsDto</returns>
        Task<GaReportsDto>  GetGaAnalyticsReports(string campaignId,string startDate,string endDate);

        /// <summary>
        /// Get Behavior Report
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>GaReportsDto</returns>
        BehaviorDto GetGaBehaviorAnalyticsReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get Conversion Report
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>GaReportsDto</returns>
        ConversionDto GetGaConversionsAnalyticsReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get TrafficSources Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        Task<ListTrafficSource>  GetTrafficSourcesReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get Traffic SourcesMediums Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        SourcesMediums GetTrafficSourcesMediumsReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get Campaign Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        Campaigns GetCampaignReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get Audience Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        Audience GetAudienceReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get DeviceCategory Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        DeviceCategory GetDeviceCategoryReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// GetGeoLocationReports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        GeoLocationDto GetGeoLocationReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Get Language Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        LanguageDto GetLanguageReports(string campaignId, string startDate, string endDate);

        /// <summary>
        /// Setup GoogleAnalytics Account
        /// </summary>
        /// <param name="id">Campaign Id</param>
        /// <returns>true or false</returns>
        Task<bool> SetupGoogleAnalyticsAccount(string id,Guid CompanyId);
        Task<Dictionary<string, string>> SetupGoogleAnalyticsAccountNew(string id, Guid CompanyId);
        Task<Dictionary<string, string>> SetupGoogleAnalyticsAccountWithJson(string id, string CompanyId);
        Task<Dictionary<string, string>> RefreshGoogleAccount(string refreshToken, string accessToken);
        GoogleTokenResponse GenerateToken();
        GoogleTokenResponse Callback(string code);
        string SetupGoogleAdsAccount();

        string PrepareGoogleAdsToken(string id);

        Task<ListTrafficSource> GetAnalyticsOrganicTraffic(string profileId,string accessToken, string startDate, string endDate);


    }





}
