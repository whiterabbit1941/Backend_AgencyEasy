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
using Stripe;
namespace EventManagement.Service
{
    public class PlanDetailService : ServiceBase<PlanDetail, Guid>, IPlanDetailService
    {

        #region PRIVATE MEMBERS

        private readonly IPlanDetailRepository _plandetailRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public PlanDetailService(IPlanDetailRepository plandetailRepository, ILogger<PlanDetailService> logger, IConfiguration configuration) : base(plandetailRepository, logger)
        {
            _plandetailRepository = plandetailRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   
         
        public  List<PlanDetailDto> getAllPlanDetails()
        {
            var allPlans = _plandetailRepository.GetAllEntities(true).Select(p => new PlanDetailDto
            {
                Id = p.Id,
                DefaultPlan = p.DefaultPlan,
                PlanId = p.DefaultPlanId,
                FeatureDto = p.Feature
               
            }).Where(y=> y.PlanId == new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26")
            || y.PlanId == new Guid ("B32218C3-427A-4CD0-B2AB-C71455E63951") 
            || y.PlanId == new Guid("3A5E8F7D-19E6-4BE7-869A-1F0ED8C907FF")
            || y.PlanId == new Guid("15640ED0-B078-4547-89B7-E2520617CFB3")
            || y.PlanId == new Guid("F3F63036-87A2-4756-AA77-ED51B6D717EB")
            || y.PlanId == new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9")
            ).ToList();
           
            foreach (var plan in allPlans)
            {
                if (plan.PlanId == new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26")
                || plan.PlanId == new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951")
                || plan.PlanId == new Guid("3A5E8F7D-19E6-4BE7-869A-1F0ED8C907FF")
                || plan.PlanId == new Guid("15640ED0-B078-4547-89B7-E2520617CFB3")
                || plan.PlanId == new Guid("F3F63036-87A2-4756-AA77-ED51B6D717EB")
                || plan.PlanId == new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"))
                 {
                    plan.FeatureList = allPlans.Where(x => x.PlanId == plan.DefaultPlan.Id).Select(y => y.FeatureDto).ToList();
                }
            }

            allPlans = allPlans.GroupBy(c => c.PlanId, (key, c) => c.FirstOrDefault()).ToList();               

            return allPlans;
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,DefaultPlanId,FeatureID,Visibility";
        }

        #endregion
    }
}
