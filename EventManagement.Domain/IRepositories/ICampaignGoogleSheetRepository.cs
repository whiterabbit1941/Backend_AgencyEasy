using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ICampaignGoogleSheetRepository : IRepository<CampaignGoogleSheet, Guid>
    {

    }
}
