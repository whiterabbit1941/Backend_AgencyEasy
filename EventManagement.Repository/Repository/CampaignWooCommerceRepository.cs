using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CampaignWooCommerceRepository : Repository<CampaignWooCommerce, Guid>, ICampaignWooCommerceRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CampaignWooCommerce> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CampaignWooCommerceRepository(EventManagementContext context, ILogger<CampaignWooCommerceRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CampaignWooCommerce>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CampaignWooCommerce> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
