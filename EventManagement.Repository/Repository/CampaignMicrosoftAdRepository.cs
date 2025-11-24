using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignMicrosoftAdRepository : Repository<CampaignMicrosoftAd, Guid>, ICampaignMicrosoftAdRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignMicrosoftAd> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignMicrosoftAdRepository(EventManagementContext context, ILogger<CampaignMicrosoftAdRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignMicrosoftAd>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignMicrosoftAd> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
