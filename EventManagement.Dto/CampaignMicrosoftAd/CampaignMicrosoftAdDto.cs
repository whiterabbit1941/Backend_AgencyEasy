using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignMicrosoftAd Model
    /// </summary>
    public class CampaignMicrosoftAdDto : CampaignMicrosoftAdAbstractBase
    {

    }

    public class TokenResponseDTO
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }

    public class MsAuthDto
    {
        public string companyId { get; set; }
        public string campaignId { get; set; }
    }

    public class MsAdAccountListDto
    {

        public long? AccountId { get; set; }
        public Guid CampaignId { get; set; }
        public string AccountName { get; set; }

    }


    public class CampaignPerformanceDto
    {
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string CampaignStatus { get; set; }
        public int Impressions { get; set; }
        public string AverageCpc { get; set; }
        public string Ctr { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
        public string ConversionRate { get; set; }
        public long Conversions { get; set; }
        public string CostPerConversion { get; set; }
        public string ImpressionSharePercent { get; set; }
        public string ImpressionLostToBudgetPercent { get; set; }
        public string ImpressionLostToRankAggPercent { get; set; }
        public string AccountStatus { get; set; }
        public string AllCostPerConversion { get; set; }
        public DateTime TimePeriod { get; set; }
        //public string ExactMatchImpressionSharePercent { get; set; }
        //public string TopImpressionSharePercent { get; set; }
        //public string AbsoluteTopImpressionSharePercent { get; set; }
    }
    public class AdGroupPerformanceDto
    {
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string AdGroupName { get; set; }
        public long AdGroupId { get; set; }
        public string Status { get; set; }

        public int Impressions { get; set; }
        public string AverageCpc { get; set; }
        public string Ctr { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
        public string ConversionRate { get; set; }
        public long Conversions { get; set; }
        public string CostPerConversion { get; set; }
        public string ImpressionSharePercent { get; set; }
        public string ImpressionLostToBudgetPercent { get; set; }
        public string ImpressionLostToRankAggPercent { get; set; }
      
        public DateTime TimePeriod { get; set; }

    }


    public class KeywordPerformanceDto
    {
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string AdGroupName { get; set; }
        public long AdGroupId { get; set; }
        public string KeywordStatus { get; set; }
        public string Keyword { get; set; }
        public int Impressions { get; set; }
        public string AverageCpc { get; set; }
        public string Ctr { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
        public string ConversionRate { get; set; }
        public long Conversions { get; set; }
        public string CostPerConversion { get; set; }

        public DateTime TimePeriod { get; set; }       
    }


    public class ConversionPerformanceDto
    {
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string Goal { get; set; }
        public decimal Revenue{ get; set; }
        public long Conversions { get; set; }
        public DateTime TimePeriod { get; set; }

    }

    public class RootCampaignPerformace
    {
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public decimal spend { get; set; }
        public string conversionRate { get; set; }
        public long conversions { get; set; }
        public string costPerConversion { get; set; }
        public string impressionSharePercent { get; set; }
        public string impressionLostToBudgetPercent { get; set; }
        public string impressionLostToRankAggPercent { get; set; }

        //for previous
        public string PrevImpressions { get; set; }
        public string PrevAverageCpc { get; set; }
        public string PrevCtr { get; set; }
        public string PrevClicks { get; set; }
        public decimal PrevSpend { get; set; }
        public string PrevConversionRate { get; set; }
        public long PrevConversions { get; set; }
        public string PrevCostPerConversion { get; set; }
        public string PrevImpressionSharePercent { get; set; }
        public string PrevImpressionLostToBudgetPercent { get; set; }
        public string PrevImpressionLostToRankAggPercent { get; set; }

        //Difference
        public string DiffImpressions { get; set; }
        public string DiffAverageCpc { get; set; }
        public string DiffCtr { get; set; }
        public string DiffClicks { get; set; }
        public string DiffSpend { get; set; }
        public string DiffConversionRate { get; set; }
        public string DiffConversions { get; set; }
        public string DiffCostPerConversion { get; set; }
        public string DiffImpressionSharePercent { get; set; }
        public string DiffImpressionLostToBudgetPercent { get; set; }
        public string DiffImpressionLostToRankAggPercent { get; set; }

        public List<CampaignPerformanceDto> campaignPerformanceDto { get; set; }

        //Chart
        public List<string> dates { get; set; }
        public List<long> clickChartValue { get; set; }
        public List<int> impressionLineChartValue { get; set; }
        public List<decimal> ctrLineChartValue { get; set; }
        public List<decimal> avgcpcLineChartValue { get; set; }
        public List<decimal> costLineChartValue { get; set; }
        public List<long> conversionLineChartValue { get; set; }
        public List<decimal> conversionRateLineChartValue { get; set; }
        public List<decimal> costPerConversionLineChartValue { get; set; }
        public List<decimal> impressionShareLineChartValue { get; set; }
        public List<decimal> impressionShareBudgetLineChartValue { get; set; }
        public List<decimal> impressionShareRankLineChartValue { get; set; }

        //Bar Chart
        public List<string> campaignsName { get; set; }
        public List<long> clickBarChartValue { get; set; }
        public List<int> impressionBarChartValue { get; set; }
        public List<decimal> ctrBarChartValue { get; set; }
        public List<decimal> avgcpcBarChartValue { get; set; }
        public List<decimal> costBarChartValue { get; set; }
        public List<long> conversionBarChartValue { get; set; }
        public List<decimal> conversionRateBarChartValue { get; set; }
        public List<decimal> costPerConversionBarChartValue { get; set; }
        public List<decimal> impressionShareBarChartValue { get; set; }
        public List<decimal> impressionShareBudgetBarChartValue { get; set; }
        public List<decimal> impressionShareRankBarChartValue { get; set; }

        public HttpStatusCode statusCode { get; set; }

        public string errorMessage { get; set; }
    }

    public class RootAdGroupPerformance
    {
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public decimal spend { get; set; }
        public string conversionRate { get; set; }
        public long conversions { get; set; }
        public string costPerConversion { get; set; }
        public string impressionSharePercent { get; set; }
        public string impressionLostToBudgetPercent { get; set; }
        public string impressionLostToRankAggPercent { get; set; }
        public List<AdGroupPerformanceDto> adGroupPerformanceDto { get; set; }

        //for previous
        public string PrevImpressions { get; set; }
        public string PrevAverageCpc { get; set; }
        public string PrevCtr { get; set; }
        public string PrevClicks { get; set; }
        public decimal PrevSpend { get; set; }
        public string PrevConversionRate { get; set; }
        public long PrevConversions { get; set; }
        public string PrevCostPerConversion { get; set; }
        public string PrevImpressionSharePercent { get; set; }
        public string PrevImpressionLostToBudgetPercent { get; set; }
        public string PrevImpressionLostToRankAggPercent { get; set; }

        //Difference
        public string DiffImpressions { get; set; }
        public string DiffAverageCpc { get; set; }
        public string DiffCtr { get; set; }
        public string DiffClicks { get; set; }
        public string DiffSpend { get; set; }
        public string DiffConversionRate { get; set; }
        public string DiffConversions { get; set; }
        public string DiffCostPerConversion { get; set; }
        public string DiffImpressionSharePercent { get; set; }
        public string DiffImpressionLostToBudgetPercent { get; set; }
        public string DiffImpressionLostToRankAggPercent { get; set; }

        //Chart
        public List<string> dates { get; set; }
        public List<long> clickChartValue { get; set; }
        public List<int> impressionLineChartValue { get; set; }
        public List<decimal> ctrLineChartValue { get; set; }
        public List<decimal> avgcpcLineChartValue { get; set; }
        public List<decimal> costLineChartValue { get; set; }
        public List<long> conversionLineChartValue { get; set; }
        public List<decimal> conversionRateLineChartValue { get; set; }
        public List<decimal> costPerConversionLineChartValue { get; set; }
        public List<decimal> impressionShareLineChartValue { get; set; }
        public List<decimal> impressionShareBudgetLineChartValue { get; set; }
        public List<decimal> impressionShareRankLineChartValue { get; set; }

        //Bar Chart
        public List<string> adGroupName { get; set; }
        public List<long> clickBarChartValue { get; set; }
        public List<int> impressionBarChartValue { get; set; }
        public List<decimal> ctrBarChartValue { get; set; }
        public List<decimal> avgcpcBarChartValue { get; set; }
        public List<decimal> costBarChartValue { get; set; }
        public List<long> conversionBarChartValue { get; set; }
        public List<decimal> conversionRateBarChartValue { get; set; }
        public List<decimal> costPerConversionBarChartValue { get; set; }
        public List<decimal> impressionShareBarChartValue { get; set; }
        public List<decimal> impressionShareBudgetBarChartValue { get; set; }
        public List<decimal> impressionShareRankBarChartValue { get; set; }

        public HttpStatusCode statusCode { get; set; }
    }

    public class RootKeywordPerformance
    {
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public decimal spend { get; set; }
        public string conversionRate { get; set; }
        public long conversions { get; set; }
        public string costPerConversion { get; set; }

        //for previous
        public string PrevImpressions { get; set; }
        public string PrevAverageCpc { get; set; }
        public string PrevCtr { get; set; }
        public string PrevClicks { get; set; }
        public decimal PrevSpend { get; set; }
        public string PrevConversionRate { get; set; }
        public long PrevConversions { get; set; }
        public string PrevCostPerConversion { get; set; }

        //Difference
        public string DiffImpressions { get; set; }
        public string DiffAverageCpc { get; set; }
        public string DiffCtr { get; set; }
        public string DiffClicks { get; set; }
        public string DiffSpend { get; set; }
        public string DiffConversionRate { get; set; }
        public string DiffConversions { get; set; }
        public string DiffCostPerConversion { get; set; }


        public List<KeywordPerformanceDto> keywordPerformanceDto { get; set; }

        //Chart
        public List<string> dates { get; set; }
        public List<long> clickChartValue { get; set; }
        public List<int> impressionLineChartValue { get; set; }
        public List<decimal> ctrLineChartValue { get; set; }
        public List<decimal> avgcpcLineChartValue { get; set; }
        public List<decimal> costLineChartValue { get; set; }
        public List<long> conversionLineChartValue { get; set; }
        public List<decimal> conversionRateLineChartValue { get; set; }
        public List<decimal> costPerConversionLineChartValue { get; set; }
        public List<decimal> impressionShareLineChartValue { get; set; }
        public List<decimal> impressionShareBudgetLineChartValue { get; set; }
        public List<decimal> impressionShareRankLineChartValue { get; set; }

        //Bar Chart
        public List<string> campaignsName { get; set; }
        public List<long> clickBarChartValue { get; set; }
        public List<string> clickBarChartLabel { get; set; }

        public List<int> impressionBarChartValue { get; set; }
        public List<string> impressionBarChartLabel { get; set; }

        public List<decimal> ctrBarChartValue { get; set; }
        public List<string> ctrBarChartLabel { get; set; }

        public List<decimal> avgcpcBarChartValue { get; set; }
        public List<string> avgcpcBarChartLabel { get; set; }

        public List<decimal> costBarChartValue { get; set; }
        public List<string> costBarChartLabel { get; set; }

        public List<long> conversionBarChartValue { get; set; }
        public List<string> conversionBarChartLabel { get; set; }

        public List<decimal> conversionRateBarChartValue { get; set; }
        public List<string> conversionRateBarChartLabel { get; set; }

        public List<decimal> costPerConversionBarChartValue { get; set; }
        public List<string> costPerConversionBarChartLabel { get; set; }      

        public HttpStatusCode statusCode { get; set; }
    }

    public class RootConversionPerformance
    {
        public long conversions { get; set; }
        public decimal revenue { get; set; }

        public long prevConversions { get; set; }
        public decimal prevRevenue { get; set; }

        public string diffConversions { get; set; }
        public string diffRevenue { get; set; }

        public List<ConversionPerformanceDto> conversionPerformanceDto { get; set; }

        //Chart
        public List<string> dates { get; set; }       
        public List<long> conversionLineChartValue { get; set; }
        public List<decimal> RevenueLineChartValue { get; set; }

        //Bar Chart
        public List<string> campaignsName { get; set; }
        public List<long> conversionBarChartValue { get; set; }
        public List<decimal> revenueBarChartValue { get; set; }
      
        public HttpStatusCode statusCode { get; set; }
    }

    public class MsAdsPreviewData
    {
        public  RootCampaignPerformace RootCampaignPerformace { get; set; }
        public  RootAdGroupPerformance RootAdGroupPerformance { get; set; }
        public  RootKeywordPerformance RootKeywordPerformance { get; set; }
        public  RootConversionPerformance RootConversionPerformance { get; set; }

        public List<RootCampaignPerformace> RootSingleCampaignPerformace { get; set; }
        public List<RootAdGroupPerformance> RootSingleAdGroupPerformance { get; set; }
        public List<RootKeywordPerformance> RootSingleKeywordPerformance { get; set; }
        public List<RootConversionPerformance> RootSingleConversionPerformance { get; set; }

    }

    public class MsAdCampaignList
    {
        public long? Id { get; set; }

        public string Name { get; set; }
    }
}
