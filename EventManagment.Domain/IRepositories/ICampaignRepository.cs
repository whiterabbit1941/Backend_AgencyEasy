using System;
using EventManagment.Domain.Entities;

namespace EventManagment.Domain
{
    public interface ICampaignRepository : IRepository<Campaign, Guid>
    {

    }
}
