using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICompanyPlanService : IService<CompanyPlan, Guid>
    {
        Task<bool> createCompanyPlan(string companyId, bool isFree, [Optional] string paymentId, [Optional] string PlanName, bool Downgrade = false);

        Task<bool> CreateStripePlan(CompanyPlanDetailDto companyplan);
        Task<AppsumoPlanDetailDto> GetAppsumoPlanByCompanyPlanId(Guid id);
        Task<List<CompanyTransactionDto>> GetAllTransactionHistoryByCompanyId(Guid id);

        DefaultPlanDto GetDefaultPlanForAppsumo(string planName);
        Task<bool> IsInvoiceExists(string id);
    }
}
