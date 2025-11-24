using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class AppsumoPlanRepository : Repository<AppsumoPlan, Guid>, IAppsumoPlanRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<AppsumoPlan> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public AppsumoPlanRepository(EventManagementContext context, ILogger<AppsumoPlanRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<AppsumoPlan>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<AppsumoPlan> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
