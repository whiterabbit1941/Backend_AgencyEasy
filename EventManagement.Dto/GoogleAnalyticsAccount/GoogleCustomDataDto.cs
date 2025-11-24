using System.Collections.Generic;
namespace EventManagement.Dto
{
    public class GoogleCustomDataDto
    {                   
            public string Name { get; set; }
            public string Sessions { get; set; }
            public string Users { get; set; }
            public string Pageviews { get; set; }
            public string PercentNewSessions { get; set; }
            public string BounceRate { get; set; }
            public string PageviewsPerSession { get; set; }
            public string AvgSessionDuration { get; set; }
            public string GoalCompletionsAll { get; set; }
            public string GoalConversionRateAll { get; set; }                  
            public string Date { get; set; }            
    }    

    public class GaPreparedDataDto
    {
        public List<string> Sessions { get; set; }
        public List<string> Users { get; set; }
        public List<string> Pageviews { get; set; }
        public List<string> PercentNewSessions { get; set; }
        public List<string> BounceRate { get; set; }
        public List<string> PageviewsPerSession { get; set; }
        public List<string> AvgSessionDuration { get; set; }
        public List<string> GoalCompletionsAll { get; set; }
        public List<string> GoalConversionRateAll { get; set; }
        public List<string>  Date { get; set; }

    }

    public class TrafficSources
    {
        public GoogleCustomDataDto Direct { get; set; }

        public GoogleCustomDataDto Organic { get; set; }

        public GoogleCustomDataDto Referral { get; set; }

        public GoogleCustomDataDto Social { get; set; }

        public GoogleCustomDataDto Display { get; set; }
    }

    public class SourcesMediums
    {
        public List<GoogleCustomDataDto> SourcemediumList { get; set; }
    }

    public class Campaigns
    {
        public List<GoogleCustomDataDto> Campaign { get; set; }

        public GoogleCustomDataDto CampaignNotSet { get; set; }
    }

    public class Audience
    {
        public GoogleCustomDataDto NewVisitors { get; set; }

        public GoogleCustomDataDto ReturnVisitors { get; set; }
    }

    public class DeviceCategory
    {
        public GoogleCustomDataDto Mobile { get; set; }

        public GoogleCustomDataDto Desktop { get; set; }

        public GoogleCustomDataDto Tablet { get; set; }
    }

    public class GeoLocationDto
    {
        public List<GoogleCustomDataDto>  GeoLocation { get; set; }
    }

    public class LanguageDto
    {
        public List<GoogleCustomDataDto> Language { get; set; }
    }

    public class BehaviorDto
    {
        public List<GoogleCustomDataDto> Behavior { get; set; }

        public List<GoogleCustomDataDto> LandingPages { get; set; }

        public List<SiteSpeed> SiteSpeed { get; set; }

    }

    public class ConversionDto
    {
        public ConversionPrepared Conversion { get; set; }

        public ConversionPrepared Ecommerce { get; set; }

        public GoogleCustomDataDto GoalConversion { get; set; }
    }

    public class GaReportsDto
    {
        public  GaPreparedDataDto GaPreparedDataDto { get; set; }
        public GoogleCustomDataDto GoogleCustomDataDto { get; set; }
            
    }

    public class ListTrafficSource
    {
        public GaPreparedDataDto Source { get; set; }
        public GaPreparedDataDto Medium { get; set; }
        public GaPreparedDataDto Referral { get; set; }
        public GaPreparedDataDto Social { get; set; }
        public GaPreparedDataDto Display { get; set; }

        public TrafficSources TotalData { get; set; }

    }

    public class SiteSpeed
    {
        public string Name { get; set; }
        public string Pageviews { get; set; }

        public string UniquePageviews { get; set; }

        public string AvgSessionDuration { get; set; }

        public string DomInteractiveTime { get; set; }

        public string DomLatencyMatricsSample{ get; set; }

        public string PageLoadTime { get; set; }

        public string AvgPageLoadTime { get; set; }

        public string PageLoadSample { get; set; }
    }

    public class Conversion
    {
        public string Month { get; set; }
        public string Transactions { get; set; }
        public string TransactionsRevenue { get; set; }
        public string TransactionsTax { get; set; }
        public string LocalTransactionsShipping { get; set; }
    }

    public class ConversionPrepared
    {
        public List<string> Month { get; set; }
        public List<string> Transactions { get; set; }
        public List<string> TransactionsRevenue { get; set; }
        public List<string> TransactionsTax { get; set; }
        public List<string> LocalTransactionsShipping { get; set; }
    }
    public class GoogleTokenResponse {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
    }
    public class TokenGoogleUtils {
        public string AccessToken { get; set; }
        public string RealmId { get; set; }
        public string RefreshToken { get; set; }
        public long RefreshTokenExpiry { get; set; }
        public long AccessTokenExpiry { get; set; }
    }
}
