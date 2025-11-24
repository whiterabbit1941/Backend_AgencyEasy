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
    public class TrafficSummaryService : ServiceBase<TrafficSummary, Guid>, ITrafficSummaryService
    {

        #region PRIVATE MEMBERS

        private readonly ITrafficSummaryRepository _trafficsummaryRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public TrafficSummaryService(ITrafficSummaryRepository trafficsummaryRepository, ILogger<TrafficSummaryService> logger, IConfiguration configuration) : base(trafficsummaryRepository, logger)
        {
            _trafficsummaryRepository = trafficsummaryRepository;
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
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,UpdatedOn, AvragePosition, CampaignId, Month ,Campaign, Year";
        }

        #endregion
    }
}
