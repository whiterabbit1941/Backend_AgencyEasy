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
    public class FeatureService : ServiceBase<Feature, Guid>, IFeatureService
    {

        #region PRIVATE MEMBERS

        private readonly IFeatureRepository _featureRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public FeatureService(IFeatureRepository featureRepository, ILogger<FeatureService> logger, IConfiguration configuration) : base(featureRepository, logger)
        {
            _featureRepository = featureRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   


        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )}
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Descriptions";
        }

        #endregion
    }
}
