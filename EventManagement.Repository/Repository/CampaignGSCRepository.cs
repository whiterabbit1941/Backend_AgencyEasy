using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignGSCRepository : Repository<CampaignGSC, Guid>, ICampaignGSCRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignGSC> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignGSCRepository(EventManagementContext context, ILogger<CampaignGSCRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignGSC>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignGSC> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
