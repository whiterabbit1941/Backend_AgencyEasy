using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
namespace EventManagement.Service
{
    public interface ISerpService : IService<Serp, Guid>
    {

        /// <summary>
        /// GetSerpData
        /// </summary>
        /// <param name="campaignID">campaignID</param>
        /// <param name="location">location</param>
        /// <param name="keywords">keywords</param>
        /// <param name="tag">tag</param>
        /// <returns>true or false</returns>
        Task<bool> GetSerpData(string campaignID, string location,string locationname, string listKeyword, string businessName, bool localPacksStatus, string searchParam);
        Task<JArray> GetSerpLocationData(string location);
        Task<string> UpdateKeywordsStatus();
        
       /// <summary>
       /// 
       /// </summary>
       /// <param name="campaignID"></param>
       /// <param name="location"></param>
       /// <param name="locationname"></param>
       /// <param name="listKeyword"></param>
       /// <returns></returns>
        Task<ExistNotExistKeyword> GetExistNotExistKeyword(string campaignID, string location, string locationName, List<string> listKeyword);

        List<long> PrepareAvgRanking(Guid campaignID, string fromDate, string toDate,PreviousDate previousDate);

        Task UpdateSerpData(SerpDetails serpDetails);


        HttpClient GetDataForSeoClient();

        int GetKeywordsCount(string campaignID);

        int GetCompanyKeywordsCount(Guid companyId);

        List<SerpDto> GetUniqueKeywordByProject(string campaignID);

    }
    public class SerpLocations {
        public string location_code { get; set; }
        public string location_name { get; set; }
        public string location_code_parent { get; set; }
        public string country_iso_code { get; set; }
        public string location_type { get; set; }
    }
    public class SerpLocationsList {
        List<SerpLocations> SerpLocations;
    }
}
