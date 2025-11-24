using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FinanaceManagement.API.Models;
using EventManagement.Domain;
using EventManagement.Repository;

namespace EventManagement.Repository
{
    public class AspUserRepository : Repository<AspNetUsers, string>, IAspUserRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<AspNetUsers> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public AspUserRepository(EventManagementContext context, ILogger<AspNetUsers> logger) : base(context, null)
        {
            _context = context;
            _dbset = context.Set<AspNetUsers>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<AspNetUsers> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
