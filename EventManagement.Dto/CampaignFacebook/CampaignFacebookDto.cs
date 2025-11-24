using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignFacebook Model
    /// </summary>
    public class CampaignFacebookDto : CampaignFacebookAbstractBase
    {

    }
    public class FacebookResponseObject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    public class PreviousDateRange
    {
        public DateTime PreviousStartDate { get; set; }

        public DateTime PreviousEndDate { get; set; }
    }

    public class CurrentDate
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class FacebookData  {

        public int PageImpressionsTotal { get; set; }

        public double PercentPageImpression { get; set; }

        public double AvgPageImpression { get; set; }

        public int PageReachTotal { get; set; }

        public double PercentPageReach { get; set; }

        public double AvgPageReach { get; set; }

        public int ProfileViewTotal { get; set; }

        public double PercentProfileView { get; set; }

        public double AvgPageProfileView { get; set; }

        public int TotalPageLike { get; set; }

        public int TotalNewLike { get; set; }

        public double AvgPerDayLike { get; set; }

        public int PaidReach { get; set; }

        public int OrganicReach { get; set; }

        public double PercentPaidReach { get; set; }

        public double PercentOrganicReach { get; set; }  

        public List<string> CountryLabelStr { get; set; }

        public List<double> CountryDataStr { get; set; }

        public double PercentPaidLike { get; set; }

        public double PercentOrganicLike { get; set; }

        public long TopCountForCity { get; set; }

        public string ErrorMsg { get; set; }


    }

    public class CategoryList1
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Cursors1
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class FacebookList
    {
        public string access_token { get; set; }
        public string category { get; set; }
        public List<CategoryList> category_list { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public List<string> tasks { get; set; }

    }

    public class Paging1
    {
        public Cursors1 cursors { get; set; }
    }

    public class RootObjectFBData
    {
        public List<FacebookList> data { get; set; }
        public Paging1 paging { get; set; }
        public string errror_msg { get; set; }
    }

    public class Facebook_params
    {
        public string restAPIEndPoint { get; set; }

        public string accessToken { get; set; }
    }

    public class PermissionDto
    {
        public string permission { get; set; }
        public string status { get; set; }
    }

    public class PermissionDataDto
    {
        public List<PermissionDto> data { get; set; }
    }
}
 