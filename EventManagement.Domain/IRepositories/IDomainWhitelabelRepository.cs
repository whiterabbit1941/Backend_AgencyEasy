using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IDomainWhitelabelRepository : IRepository<DomainWhitelabel, Guid>
    {
        
    }
}
