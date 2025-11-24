using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignGoogleAdsRepository : Repository<CampaignGoogleAds, Guid>, ICampaignGoogleAdsRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignGoogleAds> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignGoogleAdsRepository(EventManagementContext context, ILogger<CampaignGoogleAdsRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignGoogleAds>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignGoogleAds> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
