using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IPlanService : IService<Plan, Guid>
    {
        List<PlanDto> getPlansByProductId(Guid productId);

        PlanDto GetPlansById(Guid id);
    }

}
