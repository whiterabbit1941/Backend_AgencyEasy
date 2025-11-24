using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignMailchimpRepository : Repository<CampaignMailchimp, Guid>, ICampaignMailchimpRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignMailchimp> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignMailchimpRepository(EventManagementContext context, ILogger<CampaignMailchimpRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignMailchimp>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignMailchimp> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
