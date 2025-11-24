using System;
using System.Collections.Generic;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
namespace EventManagement.Service
{
    public class ProductService : ServiceBase<Product, Guid>, IProductService
    {

        #region PRIVATE MEMBERS

        private readonly IProductRepository _productRepository;
        private readonly IConfiguration _configuration;
        private readonly ICompanyRepository _companyRepository;
        private readonly IPlanRepository _planRepository;
        #endregion
        #region CONSTRUCTOR

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger, IConfiguration configuration, IPlanRepository planRepository) : base(productRepository, logger)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _planRepository = planRepository;
        }

		#endregion
		#region PUBLIC MEMBERS   
		public List<ProductDto> GetProductByCompnayId(Guid companyId)
		{
            //then get the whole entity and map it to the Dto.
            var products = (from product in _productRepository.GetFilteredEntities()
                            where product.CompanyID == companyId
                            orderby product.CreatedOn ascending
                            select new ProductDto
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Description = product.Description,
                            }).ToList();
			return products;
		}

        /// <summary>
        /// Delete Product and Plan
        /// </summary>
        /// <param name="productid">productid</param>
        /// <returns>int</returns>
        public async Task<int> DeleteProductAndPlan(Guid productid)
        {
            int retVal = 0;
            if ( await _planRepository.ExistAsync(x => x.ProductId == productid))
            {
                retVal = await _planRepository.DeleteBulkEntityAsync(x => x.ProductId == productid);               
            }

            //delete the product from the db.
            await DeleteEntityAsync(productid);

            return retVal;
        }
		#endregion
		#region OVERRIDDEN IMPLEMENTATION
		public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,Description,CompanyID,Company";
        }

        #endregion
    }
}
