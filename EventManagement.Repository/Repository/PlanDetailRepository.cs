using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class PlanDetailRepository : Repository<PlanDetail, Guid>, IPlanDetailRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<PlanDetail> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public PlanDetailRepository(EventManagementContext context, ILogger<PlanDetailRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<PlanDetail>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<PlanDetail> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
