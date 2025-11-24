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
    public class DefaultPlanService : ServiceBase<DefaultPlan, Guid>, IDefaultPlanService
    {

        #region PRIVATE MEMBERS

        private readonly IDefaultPlanRepository _defaultplanRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public DefaultPlanService(IDefaultPlanRepository defaultplanRepository, ILogger<DefaultPlanService> logger, IConfiguration configuration) : base(defaultplanRepository, logger)
        {
            _defaultplanRepository = defaultplanRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public DefaultPlanDto GetDefaultPlan(string defaultPlanName)
        {
            var defaultPlan =  _defaultplanRepository.GetAllEntities().Where(x => x.Name == defaultPlanName).Select(plan => new DefaultPlanDto {
              Id = plan.Id,
              Name = plan.Name,
              Cost = plan.Cost,
              MaxProjects = plan.MaxProjects,
              MaxClientUsers = plan.MaxClientUsers,
              MaxTeamUsers = plan.MaxTeamUsers,
              MaxKeywordsPerProject = plan.MaxKeywordsPerProject

            }).FirstOrDefault();
            return defaultPlan;
        }

        public DefaultPlanDto GetDefaultPlanById(string defaultPlanId)
        {
            var defaultPlan = _defaultplanRepository.GetAllEntities().Where(x => x.Id == new Guid(defaultPlanId)).Select(plan => new DefaultPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Cost = plan.Cost,
                MaxProjects = plan.MaxProjects,
                MaxClientUsers = plan.MaxClientUsers,
                MaxTeamUsers = plan.MaxTeamUsers,
                MaxKeywordsPerProject = plan.MaxKeywordsPerProject

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
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,Cost,MaxProjects,MaxTeamUsers,MaxClientUsers,MaxKeywordsPerProject";
        }

        #endregion
    }
}
