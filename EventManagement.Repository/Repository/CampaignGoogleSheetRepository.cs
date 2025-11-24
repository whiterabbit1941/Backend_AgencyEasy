using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignGoogleSheetRepository : Repository<CampaignGoogleSheet, Guid>, ICampaignGoogleSheetRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignGoogleSheet> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignGoogleSheetRepository(EventManagementContext context, ILogger<CampaignGoogleSheetRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignGoogleSheet>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignGoogleSheet> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
