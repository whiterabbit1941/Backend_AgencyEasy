using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignGoogleAnalyticsRepository : Repository<CampaignGoogleAnalytics, Guid>, ICampaignGoogleAnalyticsRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignGoogleAnalytics> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignGoogleAnalyticsRepository(EventManagementContext context, ILogger<CampaignGoogleAnalyticsRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignGoogleAnalytics>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignGoogleAnalytics> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
