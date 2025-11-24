using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignFacebookService : IService<CampaignFacebook, Guid>
    {
        /// <summary>
        /// Get Facebook Report from facebook API
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>FacebookData DTO</returns>
        Task<FacebookData> GetFacebookReport(Guid campaignId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Is AccessToken Valid
        /// </summary>
        /// <param name="accessToken">accessToken</param>
        /// <returns>bool</returns>
        Task<GraphApiTokenValid> GetAccessTokenDetails(string accessToken);

        Task<RootObjectFBData> GetFaceBookPageList(Guid campaignId);

        bool IsPermissionGranted(string accessToken);
    }
}
