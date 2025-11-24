using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class GscSummaryRepository : Repository<GscSummary, Guid>, IGscSummaryRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<GscSummary> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public GscSummaryRepository(EventManagementContext context, ILogger<GscSummaryRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<GscSummary>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<GscSummary> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
