using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using FinanaceManagement.API.Models;

namespace EventManagement.Repository
{
    public class ClientPostLogoutRedirectUriRepository : Repository<ClientPostLogoutRedirectUris, int>, IClientPostLogoutRedirectUriRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<ClientPostLogoutRedirectUris> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public ClientPostLogoutRedirectUriRepository(EventManagementContext context, ILogger<ClientPostLogoutRedirectUriRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<ClientPostLogoutRedirectUris>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<ClientPostLogoutRedirectUris> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
