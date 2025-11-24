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
    public class GscSummaryService : ServiceBase<GscSummary, Guid>, IGscSummaryService
    {

        #region PRIVATE MEMBERS

        private readonly IGscSummaryRepository _gscsummaryRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public GscSummaryService(IGscSummaryRepository gscsummaryRepository, ILogger<GscSummaryService> logger, IConfiguration configuration) : base(gscsummaryRepository, logger)
        {
            _gscsummaryRepository = gscsummaryRepository;
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
