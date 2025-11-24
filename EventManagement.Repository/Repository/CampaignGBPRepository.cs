using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignGBPRepository : Repository<CampaignGBP, Guid>, ICampaignGBPRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignGBP> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignGBPRepository(EventManagementContext context, ILogger<CampaignGBPRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignGBP>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignGBP> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
