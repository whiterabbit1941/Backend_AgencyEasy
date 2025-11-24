using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IPlanRepository : IRepository<Plan, Guid>
    {

    }
}
