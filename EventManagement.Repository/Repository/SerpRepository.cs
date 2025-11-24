using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class SerpRepository : Repository<Serp, Guid>, ISerpRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Serp> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public SerpRepository(EventManagementContext context, ILogger<SerpRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Serp>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Serp> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
