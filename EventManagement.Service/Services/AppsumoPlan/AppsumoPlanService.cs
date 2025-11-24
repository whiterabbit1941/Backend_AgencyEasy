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
    public class AppsumoPlanService : ServiceBase<AppsumoPlan, Guid>, IAppsumoPlanService
    {

        #region PRIVATE MEMBERS

        private readonly IAppsumoPlanRepository _appsumoplanRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public AppsumoPlanService(IAppsumoPlanRepository appsumoplanRepository, ILogger<AppsumoPlanService> logger, IConfiguration configuration) : base(appsumoplanRepository, logger)
        {
            _appsumoplanRepository = appsumoplanRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   
        public DefaultPlanDto GetDefaultPlanByAppsumoId(string defaultPlanId)
        {
            var defaultPlan = _appsumoplanRepository.GetAllEntities().Where(x => x.AppsumoPlanId == defaultPlanId).Select(plan => new DefaultPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Cost = plan.Cost,
                MaxProjects = plan.MaxProjects,
                MaxClientUsers = plan.MaxClientUsers,
                MaxTeamUsers = plan.MaxTeamUsers,
                MaxKeywordsPerProject = plan.MaxKeywordsPerProject,
            }).FirstOrDefault();
            return defaultPlan;
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,AppsumoPlanId,Cost,MaxProjects,MaxKeywordsPerProject,WhitelabelSupport,MaxTeamUsers,MaxClientUsers";
        }

        #endregion
    }
}
