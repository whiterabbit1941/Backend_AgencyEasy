using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignFacebookAdsRepository : Repository<CampaignFacebookAds, Guid>, ICampaignFacebookAdsRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignFacebookAds> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignFacebookAdsRepository(EventManagementContext context, ILogger<CampaignFacebookAdsRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignFacebookAds>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignFacebookAds> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
