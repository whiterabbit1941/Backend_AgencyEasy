using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IEmailWhitelabelRepository : IRepository<EmailWhitelabel, Guid>
    {

    }
}
