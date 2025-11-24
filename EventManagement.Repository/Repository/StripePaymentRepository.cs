using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class StripePaymentRepository : Repository<StripePayment, Guid>, IStripePaymentRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<StripePayment> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public StripePaymentRepository(EventManagementContext context, ILogger<StripePaymentRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<StripePayment>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<StripePayment> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
