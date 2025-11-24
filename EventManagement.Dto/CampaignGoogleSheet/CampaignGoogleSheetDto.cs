using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignGoogleSheet Model
    /// </summary>
    public class CampaignGoogleSheetDto : CampaignGoogleSheetAbstractBase
    {

    }
    public class GoogleAccountDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
        public string locale { get; set; }
    }
    public class GoogleSheetSettingsDto
    {
        public int reportType { get; set; }
        public Guid campaignId { get; set; }
        public string spreadSheetId { get; set; }
        public string spreadSheetName { get; set; }
        public string spreadSheetTab { get; set; }
        public string title { get; set; }
        public string tooltip { get; set; }
        public string cell { get; set; }
        public string dateColumn { get; set; }
        public string metricColumn { get; set; }
        public string aggregator { get; set; }
        public string dateInterval { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string prevStartDate { get; set; }
        public string prevEndDate { get; set; }
        public bool isComparePrevious { get; set; }
        public string dimensionColumn { get; set; }
        public int tableRowLimit { get; set; }
        public string sortMetrics { get; set; }
        public string sortDirection { get; set; }
        public bool excludeEmptyColumns { get; set; }
        public bool groupingByDate { get; set; }

        public double chartId { get; set; }
    }
    public class ListOfSpreadSheet
    {
        public string spreadSheetId { get; set; }
        public string spreadSheetName { get; set; }
    }
    public class FileListResponse
    {
        public string NextPageToken { get; set; }
        public string Kind { get; set; }
        public bool IncompleteSearch { get; set; }
        public List<DriveFile> Files { get; set; }
    }
    public class DriveFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class DefaultFormat
    {
        public BackgroundColor BackgroundColor { get; set; }
        public Padding Padding { get; set; }
        public string VerticalAlignment { get; set; }
        public string WrapStrategy { get; set; }
        public TextFormat TextFormat { get; set; }
        public BackgroundColorStyle BackgroundColorStyle { get; set; }
    }
    public class SpreadsheetTheme
    {
        public string PrimaryFontFamily { get; set; }
        public List<ThemeColor> ThemeColors { get; set; }
    }
    public class Properties
    {
        public string Title { get; set; }
        public string Locale { get; set; }
        public string AutoRecalc { get; set; }
        public string TimeZone { get; set; }
        public DefaultFormat DefaultFormat { get; set; }
        public SpreadsheetTheme SpreadsheetTheme { get; set; }
    }
    public class BackgroundColor
    {
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
    }
    public class Padding
    {
        public int Right { get; set; }
        public int Left { get; set; }
    }
    public class ForegroundColor
    {
    }
    public class TextFormat
    {
        public ForegroundColor ForegroundColor { get; set; }
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Strikethrough { get; set; }
        public bool Underline { get; set; }
        public ForegroundColorStyle ForegroundColorStyle { get; set; }
    }
    public class BackgroundColorStyle
    {
        public BackgroundColor RgbColor { get; set; }
    }
    public class ThemeColor
    {
        public string ColorType { get; set; }
        public BackgroundColorStyle Color { get; set; }
    }
    public class ForegroundColorStyle
    {
        public string ThemeColor { get; set; }
    }
    public class RowProperties
    {
        public BackgroundColor HeaderColor { get; set; }
        public BackgroundColor FirstBandColor { get; set; }
        public BackgroundColor SecondBandColor { get; set; }
        public ForegroundColorStyle HeaderColorStyle { get; set; }
        public BackgroundColorStyle FirstBandColorStyle { get; set; }
        public BackgroundColorStyle SecondBandColorStyle { get; set; }
    }
    public class BandedRange
    {
        public long BandedRangeId { get; set; }
        public Range Range { get; set; }
        public RowProperties RowProperties { get; set; }
    }
    public class Range
    {
        public long SheetId { get; set; }
        public int StartRowIndex { get; set; }
        public int EndRowIndex { get; set; }
        public int StartColumnIndex { get; set; }
        public int EndColumnIndex { get; set; }
    }
    public class SheetProperties
    {
        public long SheetId { get; set; }
        public string Title { get; set; }
        //public int Index { get; set; }
        //public string SheetType { get; set; }
        //public GridProperties GridProperties { get; set; }
        //public List<BandedRange> BandedRanges { get; set; }
    }
    public class GridProperties
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public bool RowGroupControlAfter { get; set; }
        public bool ColumnGroupControlAfter { get; set; }
    }
    public class AllSheets
    {
        public string SpreadsheetId { get; set; }
        public Properties Properties { get; set; }
        public List<Sheet> Sheets { get; set; }
        public string SpreadsheetUrl { get; set; }
    }
    public class Sheet
    {
        public SheetProperties Properties { get; set; }
    }    

    public class GoogleSheetData
    {
        public int ReportSubType { get; set; }

        public string Title { get; set; }

        public string Aggregator { get; set; }

        public bool IsComparePrevious { get; set; }

        public string Tooltip { get; set; }

        public string DateRange { get; set; }

        private decimal _aggregationData;

        private decimal _prevAggregationData;
        public decimal AggregationData {
            get { return Decimal.Round(_aggregationData, 2); }
            set { _aggregationData = Decimal.Round(value, 2); }
        }

        public decimal PrevAggregationData
        {
            get { return Decimal.Round(_prevAggregationData, 2); }
            set { _prevAggregationData = Decimal.Round(value, 2); }
        }

        public string DiffAggregator { get; set; }

        //For Stat Data
        public string CellData { get; set; }

        //For Line Chart  And Bar chart
        public List<decimal?> YAxis { get; set; }

        public List<string> XAxis { get; set; }

        public List<decimal?> PrevYAxis { get; set; }

        public List<string> PrevXAxis { get; set; }

        //For SparkLine Chart
        public List<string> SparklineData { get; set; }

        public decimal PieChartTotal { get; set; }  

        public HttpStatusCode HttpStatusCode { get; set; }

        public string ErrorMessage { get; set; } = "Error Not Found...";

       public  TableData TableData { get; set; }

        public double ChartId { get; set; }

        public string xAxisColumnName { get; set; }

        public string yAxisColumnName { get; set; }
       
    }

    public class ExcelData
    {
        public Dictionary<string, List<string>> Columns { get; set; } = new Dictionary<string, List<string>>();
    }

    public class ValueRange
    {
        public string Range { get; set; }
        public string MajorDimension { get; set; }
        public List<List<string>> Values { get; set; }
    }

    public class SpreadsheetData
    {
        public string SpreadsheetId { get; set; }
        public List<ValueRange> ValueRanges { get; set; }
    }

    public class CellData
    {
        public string Range { get; set; }
        public string MajorDimension { get; set; }
        public List<List<string>> Values { get; set; }
    }


    public class ColumnData
    {
        public string Name { get; set; }
        public List<string> Values { get; set; } = new List<string>();
    }


    public class RowData
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }

    public class TableData
    {
        public List<ColumnData> Columns { get; set; } = new List<ColumnData>();
       
    }

    public class TableFilterOptions
    {
        public int RowLimit { get; set; }
        public string SortColumn { get; set; }
        public string SortingOrder { get; set; }
        public bool ExcludeEmptyColumns { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
