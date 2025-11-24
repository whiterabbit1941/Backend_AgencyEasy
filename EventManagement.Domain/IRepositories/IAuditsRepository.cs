using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IAuditsRepository : IRepository<Audits, Guid>
    {

    }
}
