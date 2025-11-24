using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class DefaultPlanRepository : Repository<DefaultPlan, Guid>, IDefaultPlanRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<DefaultPlan> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public DefaultPlanRepository(EventManagementContext context, ILogger<DefaultPlanRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<DefaultPlan>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<DefaultPlan> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
