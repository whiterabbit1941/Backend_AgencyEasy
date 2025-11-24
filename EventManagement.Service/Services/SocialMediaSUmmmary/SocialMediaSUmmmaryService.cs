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
    public class SocialMediaSUmmmaryService : ServiceBase<SocialMediaSUmmmary, Guid>, ISocialMediaSUmmmaryService
    {

        #region PRIVATE MEMBERS

        private readonly ISocialMediaSUmmmaryRepository _socialmediasummmaryRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public SocialMediaSUmmmaryService(ISocialMediaSUmmmaryRepository socialmediasummmaryRepository, ILogger<SocialMediaSUmmmaryService> logger, IConfiguration configuration) : base(socialmediasummmaryRepository, logger)
        {
            _socialmediasummmaryRepository = socialmediasummmaryRepository;
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
