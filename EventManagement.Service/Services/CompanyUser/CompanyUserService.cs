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
    public class CompanyUserService : ServiceBase<CompanyUser, Guid>, ICompanyUserService
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyUserRepository _companyuserRepository;
        private readonly IConfiguration _configuration;
        private readonly ICompanyService _companyService;

        #endregion


        #region CONSTRUCTOR

        public CompanyUserService(ICompanyUserRepository companyuserRepository, ILogger<CompanyUserService> logger,
            IConfiguration configuration, ICompanyService companyService) : base(companyuserRepository, logger)
        {
            _companyuserRepository = companyuserRepository;
            _configuration = configuration;
            _companyService = companyService;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public List<SuperAdminDashboard> GetAdminDashboard(bool isSuperAdmin, string userId)
        {

            var adminDashboard = _companyService.GetAdminDashboard(userId, isSuperAdmin);

            return adminDashboard;

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
