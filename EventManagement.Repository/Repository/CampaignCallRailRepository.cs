using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignCallRailRepository : Repository<CampaignCallRail, Guid>, ICampaignCallRailRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignCallRail> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignCallRailRepository(EventManagementContext context, ILogger<CampaignCallRailRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignCallRail>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignCallRail> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
