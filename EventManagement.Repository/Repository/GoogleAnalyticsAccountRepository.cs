using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class GoogleAnalyticsAccountRepository : Repository<GoogleAnalyticsAccount, Guid>, IGoogleAnalyticsAccountRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<GoogleAnalyticsAccount> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public GoogleAnalyticsAccountRepository(EventManagementContext context, ILogger<GoogleAnalyticsAccountRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<GoogleAnalyticsAccount>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<GoogleAnalyticsAccount> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
