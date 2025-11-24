using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IGoogleAnalyticsAccountRepository : IRepository<GoogleAnalyticsAccount, Guid>
    {

    }
}
