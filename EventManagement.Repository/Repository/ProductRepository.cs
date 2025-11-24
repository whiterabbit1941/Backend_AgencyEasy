using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Product> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public ProductRepository(EventManagementContext context, ILogger<ProductRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Product>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Product> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
