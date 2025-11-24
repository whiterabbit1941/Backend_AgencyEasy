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
    public class GoogleAccountSetupService : ServiceBase<GoogleAccountSetup, Guid>, IGoogleAccountSetupService
    {

        #region PRIVATE MEMBERS

        private readonly IGoogleAccountSetupRepository _googleaccountsetupRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public GoogleAccountSetupService(IGoogleAccountSetupRepository googleaccountsetupRepository, ILogger<GoogleAccountSetupService> logger, IConfiguration configuration) : base(googleaccountsetupRepository, logger)
        {
            _googleaccountsetupRepository = googleaccountsetupRepository;
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
                        { "AccessToken", new PropertyMappingValue(new List<string>() { "AccessToken" } )},
                        { "RefreshToken", new PropertyMappingValue(new List<string>() { "RefreshToken" } )},
                        { "UserId", new PropertyMappingValue(new List<string>() { "UserId" } )},
                        { "UserName", new PropertyMappingValue(new List<string>() { "UserName" } )},
                        { "IsAuthorize", new PropertyMappingValue(new List<string>() { "IsAuthorize" } )}
                    };

    }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,AccessToken,RefreshToken,UserId,UserName,IsAuthorize,CompanyId,AccountType";
        }

        #endregion
    }
}
