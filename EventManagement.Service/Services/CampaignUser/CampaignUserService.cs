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
using RestSharp;

namespace EventManagement.Service
{
    public class CampaignUserService : ServiceBase<CampaignUser, Guid>, ICampaignUserService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignUserRepository _campaignuserRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public CampaignUserService(ICampaignUserRepository campaignuserRepository, ILogger<CampaignUserService> logger, IConfiguration configuration) : base(campaignuserRepository, logger)
        {
            _campaignuserRepository = campaignuserRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS

        public dynamic GetUserbySubjectID(string sID)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;
            var client = new RestClient(uri + "/Account/");
            var request = new RestRequest("getUserBySubjectID", Method.Get);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("sID", sID);

            var response = client.GetAsync(request).Result;
            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return rootobj;
        }

        public dynamic GetUserbyEmailID(string email)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;
            var client = new RestClient(uri + "/Account/");
            var request = new RestRequest("getUserByEmailID", Method.Get);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("email", email);

            var response = client.GetAsync(request).Result;
            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return rootobj;
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
            return "Id,Name";
        }

        #endregion
    }
}
