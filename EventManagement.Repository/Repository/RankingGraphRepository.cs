using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class RankingGraphRepository : Repository<RankingGraph, Guid>, IRankingGraphRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<RankingGraph> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public RankingGraphRepository(EventManagementContext context, ILogger<RankingGraphRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<RankingGraph>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<RankingGraph> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
