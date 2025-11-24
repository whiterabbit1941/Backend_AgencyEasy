using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignUserRepository : Repository<CampaignUser, Guid>, ICampaignUserRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignUser> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignUserRepository(EventManagementContext context, ILogger<CampaignUserRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignUser>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignUser> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
