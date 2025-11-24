using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface IAuditsService : IService<Domain.Entities.Audits, Guid>
    {
        /// <summary>
        /// Settings Task of websites
        /// </summary>
        /// <param name="websiteUrl">websiteUrl</param>
        /// <returns>True or False</returns>
        Task<bool> SettingTaskOnDataForSeo(string websiteUrl,string CompanyID);

        /// <summary>
        /// Get OnPage By TaskId
        /// </summary>
        /// <param name="taskId">taskId</param>
        /// <returns>AuditData</returns>
        Task<AuditData> GetOnPageByTaskId(long taskId);

        /// Update Status for setting task
        /// </summary>
        /// <param name="taskId">taskId</param>
        /// <returns>status of the operation</returns>
        Task<int> AuditStatusUpdate(long taskId);
    }
}
