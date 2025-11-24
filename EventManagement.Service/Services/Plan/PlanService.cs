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
    public class PlanService : ServiceBase<Plan, Guid>, IPlanService
    {

        #region PRIVATE MEMBERS

        private readonly IPlanRepository _planRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public PlanService(IPlanRepository planRepository, ILogger<PlanService> logger, IConfiguration configuration) : base(planRepository, logger)
        {
            _planRepository = planRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public List<PlanDto> getPlansByProductId(Guid productId)
        {
            //then get the whole entity and map it to the Dto.
            var plans = (from plan in _planRepository.GetFilteredEntities()
                         where plan.ProductId == productId
                         orderby plan.CreatedOn ascending
                         select new PlanDto
                         {
                             Id = plan.Id,
                             Subtitle = plan.Subtitle,
                             Name = plan.Name,
                             Description = plan.Description,
                             Features = plan.Features,
                             ProductId = plan.ProductId,
                             Price = plan.Price,
                             RecommendedAgencyPrice = plan.RecommendedAgencyPrice,
                             PaymentType = plan.PaymentType,
                             Currency = plan.Currency,
                             PaymentCycle = plan.PaymentCycle,
                             stripeProductId = plan.stripeProductId,
                             priceId = plan.priceId,
                         }).ToList();
            return plans;
        }


        public PlanDto GetPlansById(Guid id)
        {
            //then get the whole entity and map it to the Dto.
            var planResult = (from plan in _planRepository.GetFilteredEntities()
                              where plan.Id == id
                              select new PlanDto
                              {
                                  Id = plan.Id,
                                  Subtitle = plan.Subtitle,
                                  Name = plan.Name,
                                  Description = plan.Description,
                                  Features = plan.Features,
                                  ProductId = plan.ProductId,
                                  Price = plan.Price,
                                  RecommendedAgencyPrice = plan.RecommendedAgencyPrice,
                                  PaymentType = plan.PaymentType,
                                  Currency = plan.Currency,
                                  PaymentCycle = plan.PaymentCycle,
                                  stripeProductId = plan.stripeProductId,
                                  priceId = plan.priceId
                              }).FirstOrDefault();
            return planResult;
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
            return "Id,Name,Subtitle,ProductId,Features,Price,RecommendedAgencyPrice,Currency,PaymentType,PaymentCycle,Product,stripeProductId,priceId,Description";
        }

        #endregion
    }
}
