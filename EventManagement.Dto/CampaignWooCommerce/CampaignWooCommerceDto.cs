using EventManagement.Domain.Migrations;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignWooCommerce Model
    /// </summary>
    public class CampaignWooCommerceDto : CampaignWooCommerceAbstractBase
    {
        public Guid Id { get; set; }
        public Guid CampaignID { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string ShopUrl { get; set; }
    }

    public class RootWcReportData
    {
        public int TotalCardInventory { get; set; }
        public int TotalCardCustomer { get; set; }
        public int TotalCardOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal[] SalesChartData { get; set; }
        public int[] OrdersChartData { get; set; }
        public Double[] ReturningCustomerChartRate { get; set; }
        public List<WooComProduct> ProductSold { get; set; }


        public decimal[] PrevSalesChartData { get; set; }
        public int[] PrevOrdersChartData { get; set; }
        public Double[] PrevReturningCustomerChartRate { get; set; }

        public string SalesChartDiff { get; set; }
        public string OrdersChartDiff { get; set; }
        public string ReturningChartRateDiff { get; set; }
        public string LocationChartDiff { get; set; }

        public double[] LocationChartData { get; set; }
        public string[] LocationChartLabel { get; set; }

        public List<string> DateLabel { get; set; }

        public string ErrorMessage { get; set; }

        public List<KeyValuePair<string, int>> Locationdata { get;set;}

        public string Currency { get; set; }
    }

   

    public class TotalsByDate
    {
        public string sales { get; set; }
        public int orders { get; set; }
        public int items { get; set; }
        public string tax { get; set; }
        public string shipping { get; set; }
        public string discount { get; set; }
        public int customers { get; set; }
    }

    public class Totals
    {
        public Dictionary<string, TotalsByDate> totals { get; set; }
    }

    public class Links
    {
        public List<AboutLink> about { get; set; }
    }

    public class AboutLink
    {
        public string href { get; set; }
    }

    public class SalesApiResponse
    {
        public HttpStatusCode status_code { get; set; }
        public string total_sales { get; set; } = "0"; // Default to "0" or any other desired value
        public string net_sales { get; set; } = "0"; // Default to "0" or any other desired value
        public string average_sales { get; set; } = "0"; // Default to "0" or any other desired value
        public int total_orders { get; set; } = 0; // Default to 0 or any other desired value
        public int total_items { get; set; } = 0; // Default to 0 or any other desired value
        public string total_tax { get; set; } = "0"; // Default to "0" or any other desired value
        public string total_shipping { get; set; } = "0"; // Default to "0" or any other desired value
        public double total_refunds { get; set; } = 0; // Default to 0 or any other desired value
        public string total_discount { get; set; } = "0"; // Default to "0" or any other desired value
        public string totals_grouped_by { get; set; } = string.Empty; // Default to an empty string or any other desired value
        public Dictionary<string, TotalsByDate> totals { get; set; } = new Dictionary<string, TotalsByDate>(); // Default to an empty dictionary or any other desired value
        public int total_customers { get; set; } = 0; // Default to 0 or any other desired value
        public Links _links { get; set; } = new Links(); // Default to a new instance of Links or any other desired value

    }

    public class TotalsObject
    {
        public string slug { get; set; }
        public string name { get; set; }
        public int total { get; set; }
    }

    public class Products
    {
        //price : Current product price.

        //regular_price : Product regular price.

        //sale_price : Product sale price.
        public int id { get; set; }
        public string price { get; set; }
        public string regular_price { get; set; }
        public string sale_price { get; set; }
        public string price_html { get; set; }
    }

    public class WooComProduct
    {
        public string name { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public double total_revenue_per_product{ get; set; }
    }

    public class WcOrders
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
        public string version { get; set; }
        public bool prices_include_tax { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
        public string discount_total { get; set; }
        public string discount_tax { get; set; }
        public string shipping_total { get; set; }
        public string shipping_tax { get; set; }
        public string cart_tax { get; set; }
        public string total { get; set; }
        public string total_tax { get; set; }
        public int customer_id { get; set; }
        public string order_key { get; set; }
        public string payment_method { get; set; }
        public string payment_method_title { get; set; }
        public string transaction_id { get; set; }
        public string customer_ip_address { get; set; }
        public string customer_user_agent { get; set; }
        public string created_via { get; set; }
        public string customer_note { get; set; }
        public object date_completed { get; set; }
        public object date_paid { get; set; }
        public string cart_hash { get; set; }
        public string number { get; set; }
        public List<object> tax_lines { get; set; }
        public List<object> shipping_lines { get; set; }
        public List<object> fee_lines { get; set; }
        public List<object> coupon_lines { get; set; }
        public List<object> refunds { get; set; }
        public string payment_url { get; set; }
        public bool is_editable { get; set; }
        public bool needs_payment { get; set; }
        public bool needs_processing { get; set; }
        public DateTime date_created_gmt { get; set; }
        public DateTime date_modified_gmt { get; set; }
        public object date_completed_gmt { get; set; }
        public object date_paid_gmt { get; set; }
        public string currency_symbol { get; set; }

        public billing billing { get; set; }
    }

    public class billing
    {
        public string country { get; set; }
    }

    public class IsoDate
    {
        public List<DateTime> startDates { get; set; }

        public List<DateTime> endDates { get; set; }
    }

    public class OrderReturningAndLocation
    {
        public List<KeyValuePair<DateTime, double>> ReturningRates { get; set; }

        public List<KeyValuePair<string, int>> LocationList { get; set; }

        public double[] LocationChartData { get; set; }

        public string Currency {get;set;}

        public int TotalCustomer { get; set; }

    }
    public class WcValidate
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public string ErrorMessage { get; set; }
    }

}
