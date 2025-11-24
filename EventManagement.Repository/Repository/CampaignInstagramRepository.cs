using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignInstagramRepository : Repository<CampaignInstagram, Guid>, ICampaignInstagramRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignInstagram> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignInstagramRepository(EventManagementContext context, ILogger<CampaignInstagramRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignInstagram>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignInstagram> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
