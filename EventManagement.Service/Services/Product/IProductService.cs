using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IProductService : IService<Product, Guid>
    {
        List<ProductDto> GetProductByCompnayId(Guid companyId);

        /// <summary>
        /// Delete Product and Plan
        /// </summary>
        /// <param name="productid">productid</param>
        /// <returns>int</returns>
        Task<int> DeleteProductAndPlan(Guid productid);


    }
}
