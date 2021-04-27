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
        GetTinkoffData getTinkoffData = new GetTinkoffData();
        MarketDataCollector marketDataCollector = new MarketDataCollector();
        GetStocksHistory getStocksHistory = new GetStocksHistory();
        VolumeProfileScreener volumeProfileScreener = new VolumeProfileScreener();
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


            async Task NewMethod(MarketDataCollector marketDataCollector)
            {
                List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                //List<Instrument> instrumentList = new List<Instrument>();
                //var xxx = await marketDataCollector.GetInstrumentByFigi("BBG000BPL8G3");
                //instrumentList.Add(xxx);

                List<CandlesList> candlesList = new List<CandlesList>();
                foreach (var item in instrumentList)
                {
                    var candles = await marketDataCollector.GetCandlesAsync(item.Figi, CandleInterval.Hour, new DateTime(2020, 4, 26));
                    if (candles.Candles.Count == 0)
                    {
                        continue;
                    }
                    candlesList.Add(candles);
                }
                List<CandlesProfileList> profileList = volumeProfileScreener.CreateProfilesList(candlesList, 50, VolumeProfileMethod.All);

                List<CandlesProfileList> profilesList2 = volumeProfileScreener.BargainingOnPrice(profileList, 5);

                List <CandlesProfileList> profilesList1 = volumeProfileScreener.OrderVolBargaining(profilesList2);
    
                Log.Information("Start set ticker");
                foreach (var item in profilesList1)
                {
                    VolumeProfile maxVol = item.VolumeProfiles.OrderByDescending(x => (x.VolumeGreen + x.VolumeRed)).FirstOrDefault();
                    Instrument instrument = await marketDataCollector.GetInstrumentByFigi(item.Figi);
                    decimal volGreenWeight = volumeProfileScreener.RevWeightGreen(maxVol);
                    decimal volRedWeight = 100 - volGreenWeight;

                    using (StreamWriter sw = new StreamWriter("tickers", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                        sw.WriteLine();
                    }
                }
            }


            //foreach (var x in vps)
            //{
            //    Console.WriteLine(x.UpperBound);
            //    Console.WriteLine(x.LowerBound);
            //    Console.WriteLine(x.Volume);
            //    Console.WriteLine();

            //}


            MishMashScreener mishMashScreener = new MishMashScreener();

            //try
            //{
                await NewMethod(marketDataCollector);
                //List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                //await mishMashScreener.CycleTrading(candleInterval, candlesCount, margin, instrumentList);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex.Message);
            //    Log.Error(ex.StackTrace);
            //}
        }

    }
}
