using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CompanyPlanRepository : Repository<CompanyPlan, Guid>, ICompanyPlanRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CompanyPlan> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CompanyPlanRepository(EventManagementContext context, ILogger<CompanyPlanRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CompanyPlan>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CompanyPlan> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
