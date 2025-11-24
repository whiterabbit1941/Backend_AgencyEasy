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
using MailChimp.Net.Models;
using RestSharp;
using Method = RestSharp.Method;
using Google.Apis.Analytics.v3.Data;
using Google.Api.Ads.AdWords.Util.Reports.v201809;
using ThirdParty.Json.LitJson;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;

namespace EventManagement.Service
{
    public class CampaignWooCommerceService : ServiceBase<CampaignWooCommerce, Guid>, ICampaignWooCommerceService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignWooCommerceRepository _campaignwoocommerceRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public CampaignWooCommerceService(ICampaignWooCommerceRepository campaignwoocommerceRepository, ILogger<CampaignWooCommerceService> logger, IConfiguration configuration) : base(campaignwoocommerceRepository, logger)
        {
            _campaignwoocommerceRepository = campaignwoocommerceRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<RootWcReportData> GetWcReports(Guid campaignId, string startDate, string endDate)
        {
            var retVal = new RootWcReportData();
            var isMonthly = false;

            DateTime prevStartDate = DateTime.Parse(startDate);
            DateTime prevEndDate = DateTime.Parse(endDate);


            var campaign = _campaignwoocommerceRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            var reportTask = CallReportSaleApi(campaign, startDate, endDate);

            var productTask =  GetProductCountApi(campaign, startDate, endDate);

            var productSalesInfoTask =  GetProductSalesInfoApi(campaign, startDate, endDate);

            var returningRateAndLocationTask = CalculateReturningRate(campaign, startDate, endDate);

            var previousDate = CalculatePreviousStartDateAndEndDate(prevStartDate, prevEndDate);

            var prevReturningRateTask =  CalculateReturningRate(campaign, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"),
                previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

            //Previos report
            var prevReportTask = CallReportSaleApi(campaign, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

            await Task.WhenAll(reportTask, prevReportTask, productTask,
                productSalesInfoTask, returningRateAndLocationTask, prevReturningRateTask);

            // Access the results when both calls are finished
            var reportData = await reportTask;
            var prevReportData = await prevReportTask;
            var productCount = await productTask;
            var productSalesInfo = await productSalesInfoTask;
            var returningRateAndLocation = await returningRateAndLocationTask;
            var prevReturningRate = await prevReturningRateTask;

            // Extract list of dates from the totals dictionary
            List<string> dateList = new List<string>(reportData.totals.Keys);

            List<string> formattedDateList = dateList.Select(dateStr =>
            {
                DateTime parsedDate;
                if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    // The input date string is in "yyyy-MM-dd" format
                    return parsedDate.ToString("MM-dd");
                }
                else if (DateTime.TryParseExact(dateStr, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    isMonthly = true;
                    // The input date string is in "yyyy-MM" format
                    return parsedDate.ToString("yyyy-MM");
                }
                else
                {
                    // Handle invalid date string or other formats
                    return "Invalid Date";
                }
            }).ToList();

            var allSales = reportData.totals.Values.Select(totalsByDate => decimal.Parse(totalsByDate.sales)).ToArray();

            var allOrders = reportData.totals.Values.Select(totalsByDate => totalsByDate.orders).ToArray();

            var preAllSales = prevReportData.totals.Values.Select(totalsByDate => decimal.Parse(totalsByDate.sales)).ToArray();

            var preAllOrders = prevReportData.totals.Values.Select(totalsByDate => totalsByDate.orders).ToArray();

            if (isMonthly)
            {
                retVal.ReturningCustomerChartRate = returningRateAndLocation.ReturningRates
                .GroupBy(pair => new { Year = pair.Key.Year, Month = pair.Key.Month })
                .Select(group => group.Sum(pair => pair.Value)).ToArray();

                retVal.PrevReturningCustomerChartRate = prevReturningRate.ReturningRates
               .GroupBy(pair => new { Year = pair.Key.Year, Month = pair.Key.Month })
               .Select(group => group.Sum(pair => pair.Value)).ToArray();
            }
            else
            {
                retVal.ReturningCustomerChartRate = returningRateAndLocation.ReturningRates.Select(x => x.Value).ToArray();
                retVal.PrevReturningCustomerChartRate = prevReturningRate.ReturningRates.Select(x => x.Value).ToArray();
            }

            retVal.TotalCardInventory = productCount;

            retVal.TotalCardCustomer = returningRateAndLocation.TotalCustomer;

            retVal.TotalCardOrders = reportData.total_orders;

            var avgOrderValue = reportData.total_orders > 0
                               ? (Decimal.Parse(reportData.total_sales) / reportData.total_orders)
                               : 0;

            retVal.AvgOrderValue = Math.Round(avgOrderValue, 2);
            retVal.ProductSold = productSalesInfo;

            retVal.OrdersChartData = allOrders;

            retVal.SalesChartData = allSales;

            retVal.PrevOrdersChartData = preAllOrders;

            retVal.PrevSalesChartData = preAllSales;

            retVal.OrdersChartDiff = PrepareDifference(reportData.total_orders, prevReportData.total_orders);

            // Convert the string to a double
            double totalSalesDouble = double.Parse(reportData.total_sales);

            double prevTotalSalesDouble = double.Parse(prevReportData.total_sales);

            retVal.SalesChartDiff = PrepareDifference((int)totalSalesDouble,
                                    (int)prevTotalSalesDouble);

            retVal.ReturningChartRateDiff = PrepareDifference((int)returningRateAndLocation.ReturningRates.Sum(x=>x.Value), (int)prevReturningRate.ReturningRates.Sum(y=>y.Value));

            retVal.LocationChartLabel = returningRateAndLocation.LocationList.Select(x => x.Key).ToArray();

            retVal.LocationChartData = returningRateAndLocation.LocationChartData;

            retVal.DateLabel = formattedDateList;

            retVal.Locationdata = returningRateAndLocation.LocationList;

            retVal.Currency = returningRateAndLocation.Currency;

            return retVal;
        }

        public async Task<WcValidate> VaidateWcShop(string shopUrl, string consumerKey, string consumerSecret)
        {
            var retVal = new WcValidate();

            var baseUrl = shopUrl + "/wp-json/wc/v3/reports/orders";

            var client = new RestClient(baseUrl);

            var request = new RestRequest("/totals", Method.Get);
            request.AddQueryParameter("consumer_key", consumerKey);
            request.AddQueryParameter("consumer_secret", consumerSecret);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
            {
                retVal.HttpStatusCode = response.StatusCode;
                retVal.ErrorMessage = response.ErrorMessage;
            }
            else
            {
                retVal.HttpStatusCode = response.StatusCode;
                retVal.ErrorMessage = response.ErrorMessage;
            }

            return retVal;
        }
        public string GetCurrencyCodeByIso(string currency)
        {
            var retVal = string.Empty;
            var currencyCode = new List<Currency>();
            var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
            var restRequest = new RestRequest("/currency_code.json", Method.Get);

            var responseCode = restClient.GetAsync(restRequest).Result;
            if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
            {
                currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
            }

            var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();

            if (currency_symbol != null)
            {
                retVal = currency_symbol;

            }

            return retVal;
        }

        public string PrepareDifference(int current, int previous)
        {
            int difference = current - previous;
            string sign = difference >= 0 ? "+" : "-";
            return $"{current}({sign}{Math.Abs(difference)})";
        }

        private async Task<OrderReturningAndLocation> CalculateReturningRate(CampaignWooCommerce campaign, string startDate, string endDate)
        {
            var retVal = new OrderReturningAndLocation();
            if (campaign != null)
            {
                var perPage = 100; // Set the number of records to retrieve per page
                var page = 1; // Start with page 1

                var allOrderData = new List<WcOrders>(); // To store all orders

                var baseUrl = campaign.ShopUrl + "/wp-json/wc/v3";
                var client = new RestClient(baseUrl);

                while (true)
                {
                    var request = new RestRequest("/orders", Method.Get);
                    request.AddQueryParameter("consumer_key", campaign.ConsumerKey);
                    request.AddQueryParameter("consumer_secret", campaign.ConsumerSecret);
                    request.AddQueryParameter("after", startDate + "T00:00:00Z");
                    request.AddQueryParameter("before", endDate + "T23:59:59Z");
                    request.AddQueryParameter("per_page", perPage.ToString());
                    request.AddQueryParameter("page", page.ToString());

                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        var orderData = JsonConvert.DeserializeObject<List<WcOrders>>(response.Content);

                        if (orderData.Count == 0 || orderData.Count < perPage)
                        {
                            allOrderData.AddRange(orderData);
                            // No more orders, break out of the loop
                            break;
                        }

                        allOrderData.AddRange(orderData);

                        // Increment the page number for the next request
                        page++;
                    }
                    else
                    {
                        retVal.LocationList = new List<KeyValuePair<string, int>>();
                        retVal.ReturningRates = new List<KeyValuePair<DateTime, double>>();
                        break;
                    }
                }

                if (allOrderData.Count() > 0)
                {
                    if (allOrderData.Count() > 0)
                    {
                        retVal.Currency = allOrderData[0].currency_symbol;
                    }

                    var dates = GetListOfIsoDates(startDate, endDate);

                    // Prepare data for returning customer rate calculation          
                    var returningCustomerRates = new List<KeyValuePair<DateTime, double>>();

                    int totalCustomers = allOrderData.Select(order => order.customer_id).Distinct().Count();

                    // Iterate through each date range and calculate returning customer rate
                    for (int i = 0; i < dates.startDates.Count; i++)
                    {
                        int returningCustomers = allOrderData
                            .GroupBy(order => order.customer_id)
                            .Count(group => group.Count() > 1 &&
                                            group.Any(order => 
                                                order.date_completed != null &&
                                                //order.status.ToString() == "completed" &&
                                                DateTime.Parse(order.date_completed.ToString()) >= dates.startDates[i] &&
                                                DateTime.Parse(order.date_completed.ToString()) <= dates.endDates[i]));

                        double returningCustomerRate = totalCustomers != 0
                            ? (double)returningCustomers / totalCustomers * 100
                            : 0;

                        returningCustomerRates.Add(new KeyValuePair<DateTime, double>
                        (dates.startDates[i], returningCustomerRate)
                        );

                        //returningCustomerRates.Add(returningCustomerRate);                      
                    }


                    retVal.ReturningRates = returningCustomerRates;

                    retVal.TotalCustomer = totalCustomers > 0 ? totalCustomers : 0;

                    //top five location

                    // Create a dictionary to store the count of each country
                    Dictionary<string, int> countryCounts = new Dictionary<string, int>();

                    // Count the occurrences of each country
                    foreach (var order in allOrderData)
                    {
                        if (order.billing != null)
                        {
                            string country = order.billing.country;
                            if (!string.IsNullOrEmpty(country))
                            {
                                if (countryCounts.ContainsKey(country))
                                {
                                    countryCounts[country]++;
                                }
                                else
                                {
                                    countryCounts[country] = 1;
                                }
                            }
                        }
                    }

                    // Get the top five countries with the highest distinct counts
                    var topFiveCountries = countryCounts.OrderByDescending(kvp => kvp.Value).Take(5).ToList();

                    // Calculate the total count
                    int totalCount = topFiveCountries.Sum(item => item.Value);

                    if (totalCount > 0)
                    {
                        // Calculate the percentage for each count
                        retVal.LocationChartData = topFiveCountries.Select(item => (double)item.Value / totalCount * 100).ToArray();
                    }

                    retVal.LocationList = topFiveCountries;
                }
                else
                {
                    retVal.ReturningRates = new List<KeyValuePair<DateTime, double>>();
                    retVal.LocationList = new List<KeyValuePair<string, int>>();
                    retVal.LocationChartData = new double[0];
                    retVal.Currency = string.Empty;
                    retVal.TotalCustomer = 0;
                }
            }

            return retVal;
        }

        public IsoDate GetListOfIsoDates(string startDateStr, string endDateStr)
        {
            var retVal = new IsoDate();

            // Define the minimum and maximum hour values
            int minHour = 0;  // Minimum hour (midnight)
            int maxHour = 23; // Maximum hour (11:59:59 PM)

            // Convert the start and end date strings to DateTime objects
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            // Create a list to store the start dates and end dates
            List<DateTime> startDatesList = new List<DateTime>();
            List<DateTime> endDatesList = new List<DateTime>();

            // Loop through the date range
            while (startDate <= endDate)
            {
                // Create the start and end dates with the minimum and maximum hours
                DateTime currentStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, minHour, 0, 0);
                DateTime currentEndDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, maxHour, 59, 59);

                //// Format the start and end dates and add them to their respective lists
                //startDatesList.Add(currentStartDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                //endDatesList.Add(currentEndDate.ToString("yyyy-MM-ddTHH:mm:ss"));


                startDatesList.Add(currentStartDate);
                endDatesList.Add(currentEndDate);

                // Move to the next day
                startDate = startDate.AddDays(1);
            }

            retVal.startDates = startDatesList;
            retVal.endDates = endDatesList;

            return retVal;
        }

        public PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            var previousDate = new PreviousDate();
            var diff = (endDate - startDate).TotalDays;
            diff = Math.Round(diff);

            previousDate.PreviousEndDate = startDate.AddDays(-1);
            previousDate.PreviousStartDate = previousDate.PreviousEndDate.AddDays(-diff);

            return previousDate;
        }

        private async Task<List<WooComProduct>> GetProductSalesInfoApi(CampaignWooCommerce campaign, string startDate, string endDate)
        {
            var retVal = new List<WooComProduct>();
            if (campaign != null)
            {
                var baseUrl = campaign.ShopUrl + "/wp-json/wc/v3/reports";

                var client = new RestClient(baseUrl);

                var request = new RestRequest("/top_sellers", Method.Get);
                request.AddQueryParameter("consumer_key", campaign.ConsumerKey);
                request.AddQueryParameter("consumer_secret", campaign.ConsumerSecret);
                request.AddQueryParameter("date_max", endDate);
                request.AddQueryParameter("date_min", startDate);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var list = JsonConvert.DeserializeObject<List<WooComProduct>>(response.Content);
                    if (list != null && list.Count > 0)
                    {
                        list = list.Take(list.Count <= 10 ? list.Count : 10).ToList();

                        var product_ids = list.Count() > 0 ? string.Join(",", list.Select(x => x.product_id).ToList()) : "";
                        //Get product price for calculate total revenue per product
                        var products = await GetProductListApi(campaign, product_ids);

                        foreach (var product in list)
                        {
                            var price = products.Where(y => y.id == product.product_id).Select(x => x.price).FirstOrDefault(); ;
                            var priceFloat = double.Parse(price);
                            product.total_revenue_per_product = (priceFloat * product.quantity);
                        }
                        retVal = list;
                    }
                }
            }

            return retVal;
        }

        private async Task<int> GetProductCountApi(CampaignWooCommerce campaign, string startDate, string endDate)
        {
            var totalProducts = 0; // To store the total number of products

            if (campaign != null)
            {
                var baseUrl = campaign.ShopUrl + "/wp-json/wc/v3";
                var client = new RestClient(baseUrl);

                var perPage = 100; // Set the number of products to retrieve per page
                var page = 1; // Start with page 1
                
                while (true)
                {
                    var request = new RestRequest("/products", Method.Get);
                    request.AddQueryParameter("consumer_key", campaign.ConsumerKey);
                    request.AddQueryParameter("consumer_secret", campaign.ConsumerSecret);
                    request.AddQueryParameter("after", startDate + "T00:00:00Z");
                    request.AddQueryParameter("before", endDate + "T23:59:59Z");
                    request.AddQueryParameter("per_page", perPage.ToString());
                    request.AddQueryParameter("page", page.ToString());

                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var productList = JsonConvert.DeserializeObject<List<Products>>(response.Content);

                        if (productList != null && productList.Count == 100)
                        {
                            totalProducts += productList.Count;

                            // Increment the page number for the next request
                            page++;
                        }
                        else if(productList != null && productList.Count < 100)
                        {
                            totalProducts = productList.Count;
                            // No more products, break out of the loop
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        // Handle the error as needed                        
                        break;
                    }
                }                
            }
            return totalProducts;
        }

        private async Task<List<Products>> GetProductListApi(CampaignWooCommerce campaign, string productIds)
        {
            var retVal = new List<Products>();

            if (campaign != null && !string.IsNullOrEmpty(productIds))
            {
                var baseUrl = campaign.ShopUrl + "/wp-json/wc/v3";

                var client = new RestClient(baseUrl);

                var request = new RestRequest("/products", Method.Get);
                request.AddQueryParameter("consumer_key", campaign.ConsumerKey);
                request.AddQueryParameter("consumer_secret", campaign.ConsumerSecret);
                request.AddQueryParameter("include", productIds);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var list = JsonConvert.DeserializeObject<List<Products>>(response.Content);
                    if (list != null && list.Count > 0)
                    {
                        retVal = list;
                    }
                }
            }

            return retVal;
        }


        private async Task<SalesApiResponse> CallReportSaleApi(CampaignWooCommerce campaign, string startDate, string endDate)
        {
            var retVal = new SalesApiResponse();

            if (campaign != null)
            {
                var baseUrl = campaign.ShopUrl + "/wp-json/wc/v3/reports";

                var client = new RestClient(baseUrl);

                var request = new RestRequest("/sales", Method.Get);
                request.AddQueryParameter("consumer_key", campaign.ConsumerKey);
                request.AddQueryParameter("consumer_secret", campaign.ConsumerSecret);
                request.AddQueryParameter("date_max", endDate);
                request.AddQueryParameter("date_min", startDate);

                var response = await client.ExecuteAsync(request);

                retVal.status_code = response.StatusCode;

                if (response.IsSuccessful)
                {
                    var list = JsonConvert.DeserializeObject<List<SalesApiResponse>>(response.Content);
                    retVal = list[0];
                }
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
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,ShopUrl,CampaignID,ConsumerKey,ConsumerSecret";
        }

        #endregion
    }
}
