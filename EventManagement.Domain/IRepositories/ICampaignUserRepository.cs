using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ICampaignUserRepository : IRepository<CampaignUser, Guid>
    {

    }
}
