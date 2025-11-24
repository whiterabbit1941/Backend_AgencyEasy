using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class EmailWhitelabelRepository : Repository<EmailWhitelabel, Guid>, IEmailWhitelabelRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<EmailWhitelabel> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public EmailWhitelabelRepository(EventManagementContext context, ILogger<EmailWhitelabelRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<EmailWhitelabel>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<EmailWhitelabel> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
