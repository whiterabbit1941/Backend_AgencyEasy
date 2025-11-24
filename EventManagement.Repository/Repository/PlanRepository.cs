using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class PlanRepository : Repository<Plan, Guid>, IPlanRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Plan> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public PlanRepository(EventManagementContext context, ILogger<PlanRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Plan>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Plan> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
