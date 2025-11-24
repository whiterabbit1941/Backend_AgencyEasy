using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// Serp Model
    /// </summary>
    public class SerpDto : SerpAbstractBase
    {
        public Guid Id { get; set; }

        public string CampaignID { get; set; }

        public long Position { get; set; }

        public long LocalPackCount { get; set; }

        public long Searches { get; set; }

        public string Location { get; set; }

        public string Keywords { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string LocationName { get; set; }
        public DateTime CreatedOn { get; set; }

        public string BusinessName { get; set; }

        public bool LocalPacksStatus { get; set; }
    }
    public class SerpPost
    {
        [JsonProperty("language_code")]
        public string LanguageCode { get; set; } = "en";

        [JsonProperty("location_name")]
        public string LocationName { get; set; }

        [JsonProperty("keyword")]
        public string Keywords { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("pingback_url")]
        public string Url { get; set; } = "https://user848e8b057e0ad34.app.vtxhub.com/api/serps/SerpWebhook?id=$id&tag=$tag";

        [JsonProperty("depth")]
        public int Depth { get; set; } = 100;
    }
    public class SerpRes
    {
        [JsonProperty("tasks")]
        public List<SerpTask> SerpTasks { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

    }
    public class SerpTask
    {
        public List<SerpResult> Result { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("data")]
        public SEData Data { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }
    }

    public class SerpDetails
    {
        public string CampaignId { get; set; }

        public long Searchs { get; set; }

        public long LocalPacksCount { get; set; }

        public long Position { get; set; }

        public Guid? TaskId { get; set; }
    }

    public class SEData
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        public string Keyword { get; set; }

        [JsonProperty("location_name")]
        public string Location { get; set; }

    }

    public class SerpResult
    {
        [JsonProperty("se_results_count")]
        public long Searches { get; set; }

        public List<Item> Items { get; set; }
    }

    public class Item
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("rank_group")]
        public long RankGroup { get; set; }

        [JsonProperty("rank_absolute")]
        public long RankAbsolute { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("breadcrumb")]
        public string Breadcrumb { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }
    }

    public class TagData
    {
        [JsonProperty("weburl")]
        public string WebUrl { get; set; }

        [JsonProperty("business_name")]
        public string BusinessName { get; set; }
    }

    //this class for exist or not exist keyword
    public class ExistNotExistKeyword
    {
        public List<string> ExistKeywords { get; set; }
        public List<string> NotExistKeywords { get; set; }
    }  
}
 