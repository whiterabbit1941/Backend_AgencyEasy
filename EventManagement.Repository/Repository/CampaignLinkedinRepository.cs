using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignLinkedinRepository : Repository<CampaignLinkedin, Guid>, ICampaignLinkedinRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignLinkedin> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignLinkedinRepository(EventManagementContext context, ILogger<CampaignLinkedinRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignLinkedin>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignLinkedin> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
