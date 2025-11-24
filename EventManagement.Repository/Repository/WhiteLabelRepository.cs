using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class WhiteLabelRepository : Repository<WhiteLabel, Guid>, IWhiteLabelRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<WhiteLabel> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public WhiteLabelRepository(EventManagementContext context, ILogger<WhiteLabelRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<WhiteLabel>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<WhiteLabel> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
