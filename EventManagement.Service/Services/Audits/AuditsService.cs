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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Collections;

namespace EventManagement.Service
{
    public class AuditsService : ServiceBase<Domain.Entities.Audits, Guid>, IAuditsService
    {

        #region PRIVATE MEMBERS

        private readonly IAuditsRepository _auditsRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public AuditsService(IAuditsRepository auditsRepository, ILogger<AuditsService> logger, IConfiguration configuration) : base(auditsRepository, logger)
        {
            _auditsRepository = auditsRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        /// <summary>
        /// Settings Task of websites
        /// </summary>
        /// <param name="websiteUrl">websiteUrl</param>
        /// <returns>True or False</returns>
        public async Task<bool> SettingTaskOnDataForSeo(string websiteUrl, string CompanyID)
        {
            bool retVal = false;
            var url = _configuration["HostedUrl"];

            var isExistsWebsites = await ExistAsync(x => x.WebsiteUrl == websiteUrl.Trim());

           
                AuditsForCreation auditsForCreation = new AuditsForCreation();

                // Get credentials from configuration
                var dataForSeoLogin = _configuration["DataForSeoLoginV3"];
                var dataForSeoPassword = _configuration["DataForSeoPasswordV3"];
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{dataForSeoLogin}:{dataForSeoPassword}"));

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.dataforseo.com/"),
                    // Use credentials from configuration - see https://my.dataforseo.com/#api_dashboard
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Basic", credentials) }
                };
                //var rnd = new Random(); //you can set as "index of post_data" your ID, string, etc. we will return it with all results.
                var postObject = new Dictionary<int, object>
                {
                    [0] = new
                    {
                        site = websiteUrl,
                        crawl_max_pages = 10,
                        pingback_url = url+"api/audits/AuditStatusUpdate?taskId=$task_id"
                    }
                };
                var taskPostResponse = await httpClient.PostAsync("v2/op_tasks_post", new StringContent(JsonConvert.SerializeObject(new { data = postObject })));
                //dynamic settingsTaskRespons = JsonConvert.DeserializeObject(await taskPostResponse.Content.ReadAsStringAsync());

                var settingsTaskResponse = JsonConvert.DeserializeObject<DataForSeoResponse>(await taskPostResponse.Content.ReadAsStringAsync());

                if (settingsTaskResponse.Status == "ok")
                {
                    auditsForCreation.IsSent = false;
                    auditsForCreation.WebsiteUrl = websiteUrl;
                    auditsForCreation.Grade = "";
                    auditsForCreation.Status = "";
                    auditsForCreation.CompanyID = CompanyID;
                    auditsForCreation.TaskId = settingsTaskResponse.Results[0].Task_Id;


                    var auditsToReturn = await CreateEntityAsync<AuditsDto, AuditsForCreation>(auditsForCreation);

                    retVal = true;
                }
                else
                {
                    return retVal;
                }            

            return retVal;
        }

        /// <summary>
        /// Get OnPage By TaskId
        /// </summary>
        /// <param name="taskId">taskId</param>
        /// <returns>AuditData</returns>
        public async Task<AuditData> GetOnPageByTaskId(long taskId)
        {
            AuditData auditData = new AuditData();

            try
            {
                var login = _configuration["DataForSeoLogin"];
                var pass = _configuration["DataForSeoPassword"];
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.dataforseo.com/"),
                    //Instead of 'login' and 'password' use your credentials from https://my.dataforseo.com/#api_dashboard
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(login+":"+pass))) }
                };

                ////Get task status
                var response = await httpClient.GetAsync("v2/op_tasks_get");
                var obj = JsonConvert.DeserializeObject<DataForSeoResponse>(await response.Content.ReadAsStringAsync());
                var status = obj.Results.Where(y => y.Task_Id == taskId).Select(x => x.Status).FirstOrDefault();

                if (obj.Status == "ok" && status == "crawled")
                {

                    //Summary
                    var summaryResponse = await httpClient.GetAsync($"v2/op_tasks_get/" + taskId);
                    var summary = JsonConvert.DeserializeObject<DataForSeoResponse>(await summaryResponse.Content.ReadAsStringAsync());

                    if (summary.Status == "ok")
                    {
                        auditData.Summary = summary.Results[0].Summary;
                    }

                    //Page
                    var pageResponse = await httpClient.GetAsync($"v2/op_tasks_get_pages/" + taskId);
                    var page = JsonConvert.DeserializeObject<DataForSeoResponse>(await pageResponse.Content.ReadAsStringAsync());
                    if (page.Status == "ok")
                    {
                        auditData.Pages = page.Results[0].Pages;
                    }

                    //broken page
                    var brokenPageRes = await httpClient.GetAsync($"v2/op_tasks_get_broken_pages/" + taskId);
                    var brokenPage = JsonConvert.DeserializeObject<DataForSeoResponse>(await brokenPageRes.Content.ReadAsStringAsync());
                    if (page.Status == "ok")
                    {
                        auditData.BrokenPages = brokenPage.Results[0].BrokenPages;
                    }

                    //duplicate page
                    var duplicatePageRes = await httpClient.GetAsync($"v2/op_tasks_get_duplicates/" + 4888178178);
                    var duplicatePage = JsonConvert.DeserializeObject<DataForSeoResponse>(await duplicatePageRes.Content.ReadAsStringAsync());
                    if(duplicatePage.Status == "ok")
                    {
                        auditData.Duplicates = duplicatePage.Results[0].Duplicates;
                    }
                    
                    //var pageAddresses=  auditData.Pages.Select(x => x.AddressRelative);                    
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            return auditData;
        }

        /// <summary>
        /// Update Status for setting task
        /// </summary>
        /// <param name="taskId">taskId</param>
        /// <returns>status of the operation</returns>
        public async Task<int> AuditStatusUpdate(long taskId)
        {
            return await _auditsRepository.UpdateBulkEntityAsync(y => new Domain.Entities.Audits { Status = "crawled" }, x => x.TaskId == taskId);
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "WebsiteUrl", new PropertyMappingValue(new List<string>() { "WebsiteUrl" } )},
                        { "Grade", new PropertyMappingValue(new List<string>() { "Grade" } ) },
                        { "IsSent", new PropertyMappingValue(new List<string>() { "IsSent" } )},
                        { "TaskId", new PropertyMappingValue(new List<string>() { "TaskId" } )},
                        { "Status", new PropertyMappingValue(new List<string>() { "Status" } )},
                        { "CreatedOn", new PropertyMappingValue(new List<string>() { "CreatedOn" } )},
                    };
    }

        public override string GetDefaultOrderByColumn()
        {
            return "Id";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,WebsiteUrl,Grade,IsSent,TaskId,Status,CreatedOn";
        }

        #endregion
    }
}
