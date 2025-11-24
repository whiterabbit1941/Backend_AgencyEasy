using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignLinkedinService : IService<CampaignLinkedin, Guid>
    {
        /// <summary>
        /// LinkedinSetup
        /// </summary>
        /// <returns>data</returns>
        string LinkedinSetup(string source,string type);

        /// <summary>
        /// GetAccessTokenUsingCode
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="source">source</param>
        /// <returns>data</returns>
        Task<LinkedinToken> GetAccessTokenUsingCode(string code, string source,string type);

        /// <summary>
        /// GetLinkedInTPages
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>data</returns>
        Task<LinkedinRoot> GetLinkedInPages(Guid campaignId);

        /// <summary>
        /// GetLinkedinPageFollowers
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>data</returns>
        string GetLinkedinPageFollowers(string campaignId);

        /// <summary>
        /// GetLinkedinTotalShareStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>data</returns>
        string GetLinkedinTotalShareStatistics(string campaignId, string startTime, string endTime);
        /// <summary>
        /// GetLinkedinTotalOrganicPaidFollowerStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>data</returns>
        string GetLinkedinTotalOrganicPaidFollowerStatistics(string campaignId, string startTime, string endTime);

        /// <summary>
        /// GetLinkedinTotalDemographicStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>data</returns>
        string GetLinkedinTotalDemographicStatistics(string campaignId);

        /// <summary>
        /// Get linkedin setup
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>linkedin setup</returns>
        List<CampaignLinkedinDto> GetCampaignLinkedinByCampaignId(string campaignId);

        RootLinkedInDataObject PrepareLinkedinEngagement(string campaignId, string startTime, string endTime);

        LinkedInDemographicChart GetLinkedinDemographicData(Guid campaignId);

        Task<LinkedinToken> GetAccessTokenUsingRefreshToken(string refresh_token);

    }
}
