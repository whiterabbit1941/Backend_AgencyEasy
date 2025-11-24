using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignRepository : Repository<Campaign, Guid>, ICampaignRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Campaign> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignRepository(EventManagementContext context, ILogger<CampaignRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Campaign>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Campaign> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
