using System;
namespace PositionService.Shared
{
    public class StockMarketData
    {
        public string Ticker { get; set; }

        public CompanyInformation CompanyInformation { get; set; }

        public PriceData PriceData { get; set; }
    }

    public class CompanyInformation
    {
        public string ShortName { get; set; }
    }

    public class PriceData
    {
        public decimal CurrentPrice { get; set; }
    }
}
