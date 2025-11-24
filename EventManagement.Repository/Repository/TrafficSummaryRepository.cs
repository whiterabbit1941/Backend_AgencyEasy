using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class TrafficSummaryRepository : Repository<TrafficSummary, Guid>, ITrafficSummaryRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<TrafficSummary> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public TrafficSummaryRepository(EventManagementContext context, ILogger<TrafficSummaryRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<TrafficSummary>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<TrafficSummary> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
