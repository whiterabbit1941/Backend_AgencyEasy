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
    public class ReportSettingService : ServiceBase<ReportSetting, Guid>, IReportSettingService
    {

        #region PRIVATE MEMBERS

        private readonly IReportSettingRepository _reportsettingRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public ReportSettingService(IReportSettingRepository reportsettingRepository, ILogger<ReportSettingService> logger, IConfiguration configuration) : base(reportsettingRepository, logger)
        {
            _reportsettingRepository = reportsettingRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public List<ReportSetting> GetReportByCampaign(string campaignId , Guid companyId)
        {
            var retVal = new List<ReportSetting>();

            if (string.IsNullOrEmpty(campaignId))
            {
                retVal = _reportsettingRepository.GetAllEntities(false).Where(x => x.CampaignId == null && x.CompanyId == companyId).ToList();

            }
            else
            {
                retVal = _reportsettingRepository.GetAllEntities(false).Where(x => x.CampaignId == new Guid(campaignId) && x.CompanyId == companyId).ToList();
            }

            return retVal;

        }
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
            return "Id,Name,CampaignId,CompanyId,StartDate,EndDate,ReportType,IsCoverPage,TableOfContent,IsPageBreak,Comments,HeaderSettings,Html,Frequency,Images";
        }

        #endregion
    }
}
