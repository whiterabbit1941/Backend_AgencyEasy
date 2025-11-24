using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class FeatureRepository : Repository<Feature, Guid>, IFeatureRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Feature> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public FeatureRepository(EventManagementContext context, ILogger<FeatureRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Feature>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Feature> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
