using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface IDefaultPlanService : IService<DefaultPlan, Guid>
    {

        DefaultPlanDto GetDefaultPlan(string defaultPlanName);

        DefaultPlanDto GetDefaultPlanById(string defaultPlanId);
    }
}
