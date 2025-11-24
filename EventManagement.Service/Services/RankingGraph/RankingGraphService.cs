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
    public class RankingGraphService : ServiceBase<RankingGraph, Guid>, IRankingGraphService
    {

        #region PRIVATE MEMBERS

        private readonly IRankingGraphRepository _rankinggraphRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public RankingGraphService(IRankingGraphRepository rankinggraphRepository, ILogger<RankingGraphService> logger, IConfiguration configuration) : base(rankinggraphRepository, logger)
        {
            _rankinggraphRepository = rankinggraphRepository;
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
