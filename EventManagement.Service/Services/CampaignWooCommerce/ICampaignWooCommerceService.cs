using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Service
{
    public interface ICampaignWooCommerceService : IService<CampaignWooCommerce, Guid>
    {
        Task<RootWcReportData> GetWcReports(Guid campaignId, string startDate, string endDate);

        Task<WcValidate> VaidateWcShop(string shopUrl, string consumerKey, string consumerSecret);
    }
}
