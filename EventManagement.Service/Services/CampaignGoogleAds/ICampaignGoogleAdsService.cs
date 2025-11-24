using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;
using Google.Protobuf.Collections;

namespace EventManagement.Service
{
    public interface ICampaignGoogleAdsService : IService<CampaignGoogleAds, Guid>
    {
        /// <summary>
        /// Gets List of google customer
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>list of customers</returns>
        public List<GoogleAdsCustomerDto> GetListOfGaAdsCustomer(string refreshToken);

        /// <summary>
        /// Get Google Ads data
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="reportType">reportType</param>
        /// <returns>reports data</returns>
        public List<GoogleAdsCampaignReport> GetGoogleAdsReports(string campaignId, string startDate, string endDate, int reportType);

        /// <summary>
        /// Update previously access token, refresh token if email id same in same company 
        /// </summary>
        /// <param name="campaignAds">campaignAds</param>
        /// <param name="companyId">companyId</param>
        /// <returns>bool</returns>
        Task<bool> UpdateRefreshTokenAndEmail(CampaignGoogleAdsForCreation campaignAds, string companyId);

        bool IsPropertiesExists(string refresh_token);
    }
}
