using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignFacebookRepository : Repository<CampaignFacebook, Guid>, ICampaignFacebookRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignFacebook> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignFacebookRepository(EventManagementContext context, ILogger<CampaignFacebookRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignFacebook>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignFacebook> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
