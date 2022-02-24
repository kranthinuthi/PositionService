using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using PositionService.Resources;
using PositionService.Dtos;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PositionService.Shared;
using System.Net.Http.Json;

namespace PositionService.Biz
{
    public class PositionManager : IPositionManager
    {
        private const string  apiKey = "2f8d667b-5be0-4a8d-9981-2a18ce406f83";
        private const string  styvioUrl = "https://www.styvio.com/apiV2/";

        public async Task<List<Position>> GetCurrentNetPositionsAsync()
        {
            var positions = new List<Position>();
            using (var reader = new StreamReader(@"transactions.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<PositionDto>().ToList();

                if (records !=null && records.Count() > 0)
                {
                    var symbolsList = records.Select(x => x.Symbol).Distinct();

                    if (symbolsList != null && symbolsList.Any())
                    {
                        var marketData = new Dictionary<string,StockMarketData>();
                        //marketData = await GetMarketDataAsync(symbolsList);
                        //To actually go to the api to retrieve market data, uncomment the above and comment the below line
                        marketData = GetDummyMarketData();
                        positions = CreateNetPositions(records, marketData);
                        return positions;
                    }
                }   
            }
            return null;
        }

        private Dictionary<string, StockMarketData> GetDummyMarketData()
        {
            var marketData = new Dictionary<string, StockMarketData>();
            marketData.Add("AMZN", new StockMarketData
            {
                Ticker = "AMZN",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "Amazon"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 3003.55m
                }
            });

            marketData.Add("PEP", new StockMarketData
            {
                Ticker = "PEP",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "PepsiCo,Inc"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 167.50m
                }
            });

            marketData.Add("AMGN", new StockMarketData
            {
                Ticker = "AMGN",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "Amgen, Inc"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 221.30m
                }
            });

            marketData.Add("MSFT", new StockMarketData
            {
                Ticker = "MSFT",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "Microsoft Corporation"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 286.78m
                }
            });

            marketData.Add("TSLA", new StockMarketData
            {
                Ticker = "TSLA",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "Tesla Inc"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 813.05m
                }
            });

            marketData.Add("MCD", new StockMarketData
            {
                Ticker = "MCD",
                CompanyInformation = new CompanyInformation
                {
                    ShortName = "McDonald's Corp"
                },
                PriceData = new PriceData
                {
                    CurrentPrice = 252.94m
                }
            });

            return marketData;
        }

        private async Task<Dictionary<string, StockMarketData>> GetMarketDataAsync(IEnumerable<string> symbolsList)
        {
            var marketData = new Dictionary<string, StockMarketData>();
            foreach (var symbol in symbolsList)
            {
                var httpClient = new HttpClient();
                var url = styvioUrl + symbol + "/" + apiKey;
                try
                {
                    marketData.Add(symbol, await httpClient.GetFromJsonAsync<StockMarketData>(url));
                }
                catch (HttpRequestException) // Non success
                {
                    Console.WriteLine($"Error occured while getting marketdata for {symbol}.");
                }
            }
            return marketData;
        }

        private List<Position> CreateNetPositions(List<PositionDto> positions, Dictionary<string, StockMarketData> marketData)
        {
            var netPositions = new List<Position>();
            var positionGroups  = positions.OrderBy(x => x.TradeDate).GroupBy(x => x.Symbol).ToList();

            foreach(var symbolGroup in positionGroups)
            {
                var symbol = symbolGroup.Key;
                var symbolPositions = symbolGroup.ToList();
                var symbolPositionGroupedBySide = symbolPositions.GroupBy(x => x.Side).ToList();
                var netPosition = new Position
                {
                    Symbol = symbolGroup.Key,
                    Name = marketData[symbolGroup.Key].CompanyInformation.ShortName,
                    CurrentPrice = marketData[symbolGroup.Key].PriceData.CurrentPrice
                };

                var buyPositions = symbolPositionGroupedBySide.Exists(x => x.Key == Side.Buy) ? symbolPositionGroupedBySide.Single(x => x.Key == Side.Buy)?.ToList() : null;
                var sellPositions = symbolPositionGroupedBySide.Exists(x => x.Key == Side.Sell) ? symbolPositionGroupedBySide.Single(x => x.Key == Side.Sell)?.ToList() : null;

                //Assuming we are not shorting here , all positions will be long

                CalculateNetQuantityAndPnl(buyPositions, sellPositions, netPosition);
                netPositions.Add(netPosition);
            }

            return netPositions;
        }

        private void CalculateNetQuantityAndPnl(List<PositionDto> buyPositions, List<PositionDto> sellPositions, Position netPosition)
        {

            long buyQuantity = buyPositions != null ? buyPositions.Sum( x => x.Quantity) : 0;
            long sellQuantity = sellPositions != null ? sellPositions.Sum(x => x.Quantity) : 0;
            netPosition.Quantity = buyQuantity - sellQuantity;

            decimal averageBuyPrice =  buyQuantity > 0 ?  buyPositions.Sum(x => x.Price * x.Quantity) / buyQuantity : 0;
            decimal averageSellPrice = sellQuantity > 0 ? sellPositions.Sum(x => x.Price * x.Quantity) / sellQuantity : 0;

            netPosition.RealizedPnl  = (averageSellPrice - averageBuyPrice) * sellQuantity;
            netPosition.UnrealizedPnl = (netPosition.CurrentPrice - averageBuyPrice) * netPosition.Quantity;
            netPosition.TotalPnl = netPosition.RealizedPnl + netPosition.UnrealizedPnl;
        }
    }
}
