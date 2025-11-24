using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IEventRepository : IRepository<Event, Guid>
    {

    }
}
