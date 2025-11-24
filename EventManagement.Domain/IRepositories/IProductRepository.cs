using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IProductRepository : IRepository<Product, Guid>
    {

    }
}
