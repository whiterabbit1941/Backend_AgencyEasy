using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignCallRail Model
    /// </summary>
    public class CampaignCallRailDto : CampaignCallRailAbstractBase
    {
        public class AccountResponse
        {
            public HttpStatusCode HttpStatus { get; set; }
            public string ErrorMessage { get; set; }

            [JsonProperty("total_records")]
            public int TotalRecords { get; set; }
            public List<Account> Accounts { get; set; } = new List<Account> { };
        }

        public class Account
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool OutboundRecordingEnabled { get; set; }
            public bool HipaaAccount { get; set; }
            public int numeric_id { get; set; }
        }

        public class CallResponse
        {
            public int page { get; set; }
            public int per_page { get; set; }
            public int total_pages { get; set; }
            public int total_records { get; set; }
            public List<Call> calls { get; set; }
            public HttpStatusCode StatusCodes { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Call
        {
            public bool answered { get; set; }          
            public string customer_city { get; set; }
            public string customer_name { get; set; }
            public string customer_phone_number { get; set; }
            public int duration { get; set; }
            public string id { get; set; }
            public string recording { get; set; }
            public int? recording_duration { get; set; }
            public string recording_player { get; set; }
            public DateTimeOffset start_time { get; set; }
            public DateTime created_at { get; set; }
            public string formattedCreatedAt => created_at.ToString("MMM d, yyyy h:mm tt");

            // Method to convert duration to "HH:mm:ss" format
            public string formattedDuration
            {
                get
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                    return timeSpan.ToString(@"hh\:mm\:ss");
                }
            }
            public string source_name { get; set; }
            public string source { get; set; }
            public bool first_call { get; set; }
            public string landing_page_url { get; set; }
            public string device_type { get; set; }
            public string lead_status { get; set; }
            public string keywords { get; set; }
            public List<string> tags { get; set; }
            public string company_name { get; set; }
            public string campaign { get; set; }
           
        }

        public class LineChart
        {
            public string Dates { get; set; }

            public int Answered { get; set; }

            public int Missed { get; set; }

            public int FirstTime { get; set; }

            public int Calls { get; set; }

            public int Duration { get; set; }

            public double AnswerRateDaily { get; set; }

            public double MissedRateDaily { get; set; }

            public double DurationGroupByAvg { get; set; }

            public double FirstTimeRateDaily { get; set; }

        }

        public class CallRailTableData
        {
            public List<Call> Call { get; set;}
            public HttpStatusCode StatusCodes { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class CallRailDashboardData
        {
            public List<string> Dates { get; set; }         
            public Dictionary<string, int> SourceCounts { get; set; }
            public int TotalAnswered { get; set; }
            public int TotalMissed { get; set; }
            public int TotalFirstTime { get; set; }
            public int TotalCalls { get; set; }
            public int TotalLeads { get; set; }
            public string AvgDuration { get; set; }
            public double AvgDurationDouble { get; set; }
            public double TotalMissedRateAvg { get; set; }
            public double TotalAnsweredRateAvg { get; set; }
            public double TotalAvgFirstTimeCallRate { get; set; }
            public double TotalAvgCallsPerLead { get; set; }


            public List<int> AnsweredList { get; set; }
            public List<int> MissedCallList { get; set; }
            public List<int> FirstTimeList { get; set; }
            public List<int> CallsList { get; set; }
            public List<int> LeadsList { get; set; }
            public List<string> DurationList { get; set; }
            public List<int> DurationListInt { get; set; }
            public List<double> AvgAnswerRateList { get; set; }
            public List<double> AvgMissedRateList { get; set; }
            public List<double> AvgFirstTimeCallRateList { get; set; }
            public List<double> AvgCallsPerLeadList { get; set; }
            public HttpStatusCode StatusCodes { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class CallReportDTO
        {
            public Guid CampaignId { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string PrevStartDate { get; set; }
            public string PrevEndDate { get; set; }
        }

        public class CallRailReportData
        {
            public CallRailDashboardData CurrentPeriodData { get; set; }

            public CallRailDashboardData PreviousPeriodData { get; set; }

            public string PieChartDiff { get; set; }

            public string CallsDiff { get; set; }

            public string AnsweredDiff { get; set; }

            public HttpStatusCode StatusCodes { get; set; }
            public string ErrorMessage { get; set; }

            public string MissedDiff { get; set; }
            public string AvgDurationDiff { get; set; }
            public string AvgDuration { get; set; }
            public string FirstTimeDiff { get; set; }
            public string AnsweredRateAvgDiff { get; set; }
            public string MissedRateAvgDiff { get; set; }
            public string FirstRateAvgDiff { get; set; }
            public string LeadsDiff { get; set; }            
            public string AvgCallPerLeadsDiff { get; set; }
        }

        public class Recording
        {
            public string url { get; set; }
            public HttpStatusCode StatusCodes { get; set; }
            public string ErrorMessage { get; set; }
        }

    }
}
        