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
using Amazon.Amplify.Model;
using SendGrid;
using System.Net;

namespace EventManagement.Service
{
    public class EmailWhitelabelService : ServiceBase<EmailWhitelabel, Guid>, IEmailWhitelabelService
    {

        #region PRIVATE MEMBERS

        private readonly IEmailWhitelabelRepository _emailwhitelabelRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public EmailWhitelabelService(IEmailWhitelabelRepository emailwhitelabelRepository, ILogger<EmailWhitelabelService> logger, IConfiguration configuration) : base(emailwhitelabelRepository, logger)
        {
            _emailwhitelabelRepository = emailwhitelabelRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   


        public async Task<int> DeleteEmailByCompanyID(Guid companyId)
        {
            var entity = GetAllEntities().Where(x => x.CompanyID == companyId).FirstOrDefault();

            if (entity != null)
            {
                var transportInstance = GetTransportMechanism();
                var DeleteResponse = await transportInstance.RequestAsync(method: SendGridClient.Method.DELETE, urlPath: "whitelabel/domains/" + entity.DomainID);
                if (DeleteResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    await DeleteBulkEntityAsync(x => x.DomainID == entity.DomainID);
                    return 1;
                }
                else
                {
                    return 0;
                }

            }
            else { return 0; }
        }

        private SendGridClient GetTransportMechanism()
        {
            return new SendGridClient(_configuration.GetSection("Client").Value);
        }
        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "DomainName", new PropertyMappingValue(new List<string>() { "DomainName" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "DomainName";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,CompanyID,DomainID,DomainName,CnameHost,CnameType,CnamePointsTo,DomainKey1Type,DomainKey1PointsTo,DomainKey2Type,DomainKey2PointsTo,IsVerify";
        }

        #endregion
    }
}
