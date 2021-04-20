using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TinkoffAdapter.DataHelper;
using DataCollector;
using CandleInterval = MarketDataModules.CandleInterval;
using ScreenerStocks;
using ScreenerStocks.Helpers;
using MarketDataModules;
using Analysis.Screeners;
using System.Linq;
using System.IO;
using TinkoffAdapter.Authority;
using MarketDataModules.Models.Candles;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 104857600, rollOnFileSizeLimit: true)
                .CreateLogger();
            MarketDataCollector marketDataCollector = new MarketDataCollector();
            GetStocksHistory getStocksHistory = new GetStocksHistory();
            VolumeProfileScreener volumeProfileScreener = new VolumeProfileScreener();
            var candleInterval = CandleInterval.Day;

            int candlesCount = 45;
            decimal margin = 9000;
            List<Instrument> instrumentList = await NewMethod(marketDataCollector, getStocksHistory, volumeProfileScreener);

            //foreach (var x in vps)
            //{
            //    Console.WriteLine(x.UpperBound);
            //    Console.WriteLine(x.LowerBound);
            //    Console.WriteLine(x.Volume);
            //    Console.WriteLine();

            //}

            Console.ReadKey();


            MishMashScreener mishMashScreener = new MishMashScreener();

            try
            {
                // List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                await mishMashScreener.CycleTrading(candleInterval, candlesCount, margin, instrumentList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }

        private static async Task<List<Instrument>> NewMethod(MarketDataCollector marketDataCollector, GetStocksHistory getStocksHistory, VolumeProfileScreener volumeProfileScreener)
        {
            List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
            foreach (var item in instrumentList)
            {
                var candles = await marketDataCollector.GetCandlesAsync(item.Figi, CandleInterval.Day, 200);
                if (candles == null)
                {
                    continue;
                }

                CandlesListProfile vps = volumeProfileScreener.VolumeProfileList(candles, 50, VolumeProfileMethod.All);
                var result = vps.VolumeProfiles.OrderByDescending(x => x.Volume);
                var finalresult = result.FirstOrDefault();
                decimal averageBound = (finalresult.UpperBound + finalresult.LowerBound) / 2;
                decimal procentUp = averageBound * 1.1M;
                decimal procentDown = averageBound * 0.9M;
                decimal close = candles.Candles.Last().Close;

                if (close < procentUp && close > procentDown)
                {
                    Console.WriteLine(candles.Figi);
                    Console.WriteLine(finalresult.UpperBound);
                    Console.WriteLine(finalresult.LowerBound);
                    Console.WriteLine(finalresult.Volume);
                    Console.WriteLine(candles.Candles.Last().Close);
                    Console.WriteLine("***");
                    using (StreamWriter sw = new StreamWriter("tickers", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(item.Ticker + " UpperBound: " + finalresult.UpperBound + " LowerBound: " + finalresult.LowerBound + " Volume: " + finalresult.Volume + " Close:" + candles.Candles.Last().Close);
                        sw.WriteLine();
                    }
                }
            }
            return instrumentList;
        }
    }
}
