using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class StripeCouponRepository : Repository<StripeCoupon, Guid>, IStripeCouponRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<StripeCoupon> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public StripeCouponRepository(EventManagementContext context, ILogger<StripeCouponRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<StripeCoupon>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<StripeCoupon> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
