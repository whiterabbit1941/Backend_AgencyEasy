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
using FinanaceManagement.API.Models;

namespace EventManagement.Service
{
    public class ClientPostLogoutRedirectUriService : ServiceBase<ClientPostLogoutRedirectUris, int>, IClientPostLogoutRedirectUriService
    {

        #region PRIVATE MEMBERS

        private readonly IClientPostLogoutRedirectUriRepository _clientpostlogoutredirecturiRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public ClientPostLogoutRedirectUriService(IClientPostLogoutRedirectUriRepository clientpostlogoutredirecturiRepository, ILogger<ClientPostLogoutRedirectUriService> logger, IConfiguration configuration) : base(clientpostlogoutredirecturiRepository, logger)
        {
            _clientpostlogoutredirecturiRepository = clientpostlogoutredirecturiRepository;
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
