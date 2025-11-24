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
    public class TemplateSettingService : ServiceBase<TemplateSetting, Guid>, ITemplateSettingService
    {

        #region PRIVATE MEMBERS

        private readonly ITemplateSettingRepository _templatesettingRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public TemplateSettingService(ITemplateSettingRepository templatesettingRepository, ILogger<TemplateSettingService> logger, IConfiguration configuration) : base(templatesettingRepository, logger)
        {
            _templatesettingRepository = templatesettingRepository;
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
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,CompanyId,StartDate,EndDate,ReportType,IsCoverPage,TableOfContent,IsPageBreak,Comments,HeaderSettings,Html,Frequency,Images";
        }

        #endregion
    }
}
