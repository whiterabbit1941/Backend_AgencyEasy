using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IPlanDetailService : IService<PlanDetail, Guid>
    {
        List<PlanDetailDto> getAllPlanDetails();
    }
}
