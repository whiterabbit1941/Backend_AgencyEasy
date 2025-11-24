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
    public class CancellationReasonService : ServiceBase<CancellationReason, Guid>, ICancellationReasonService
    {

        #region PRIVATE MEMBERS

        private readonly ICancellationReasonRepository _cancellationreasonRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public CancellationReasonService(ICancellationReasonRepository cancellationreasonRepository, ILogger<CancellationReasonService> logger, IConfiguration configuration) : base(cancellationreasonRepository, logger)
        {
            _cancellationreasonRepository = cancellationreasonRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   


        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public Guid CompanyId { get; set; }

        public string Reason { get; set; }

        public string OtherSolution { get; set; }

        public string Rating { get; set; }

        public string Feedback { get; set; }

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
            return "Id,Reason,OtherSolution,Rating,Feedback";
        }

        #endregion
    }
}
