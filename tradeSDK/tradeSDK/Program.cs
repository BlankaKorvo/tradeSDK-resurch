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
using TinkoffData;
using Skender.Stock.Indicators;
using TradingAlgorithms.IndicatorSignals;

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

            int candlesCount = 80;
            decimal maxMoneyForTrade = 9000;

            MishMashScreener mishMashScreener = new MishMashScreener();

            try
            {
                List<string> tickers = new List<string> { }; //= new List<string> { "qdel", "med", "appf", "sage", "crox", "bio", "lpx", "hear", "txn", "trow", "fizz", "rgr", "bx", "coo", "vrtx", "prg", "azpn", "bpmc", "holx", "nbix" };
                //await NewMethod(marketDataCollector);
                List<Instrument> UsdinstrumentList = await getStocksHistory.AllUsdStocksAsync();
                //List<Instrument> RubinstrumentList = await getStocksHistory.AllRubStocksAsync();
                //List<Instrument> instrumentList = UsdinstrumentList.Union(RubinstrumentList).ToList();
                var result = await mishMashScreener.GetAllTransactionModels(candleInterval, candlesCount, maxMoneyForTrade, UsdinstrumentList);

                foreach (var item in result)
                {
                    if (item.Operation == Operation.toLong)
                    { 
                        tickers.Add(item.Figi);
                        Console.WriteLine(item.Figi);
                    }
                }
                Console.ReadLine();
                //List<Instrument> instrumentList = new List<Instrument> { };
                //foreach (var item in tickers)
                //{
                //    instrumentList.Add(await marketDataCollector.GetInstrumentByTicker(item));
                //}
                //await mishMashScreener.CycleTrading(candleInterval, candlesCount, maxMoneyForTrade, instrumentList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }


            async Task NewMethod(MarketDataCollector marketDataCollector)
            {
                Signal signal = new Signal();
                List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                //List<Instrument> instrumentList = new List<Instrument>();
                //var xxx = await marketDataCollector.GetInstrumentByFigi("BBG000BPL8G3");
                //instrumentList.Add(xxx);

                var candleInterval = CandleInterval.Hour;

                List<CandlesList> candlesList = new List<CandlesList>();
                foreach (var item in instrumentList)
                {
                    var candles = await marketDataCollector.GetCandlesAsync(item.Figi, candleInterval, new DateTime(2021, 1, 1));
                    if (candles.Candles.Count == 0)
                    {
                        continue;
                    }
                    candlesList.Add(candles);
                }
                List<CandlesProfileList> profileList = volumeProfileScreener.CreateProfilesList(candlesList, 50, VolumeProfileMethod.All);

                List<CandlesProfileList> profilesList2 = volumeProfileScreener.BargainingOnPrice(profileList, 10);

                List <CandlesProfileList> profilesList1 = volumeProfileScreener.OrderVolBargaining(profilesList2);
    
                Log.Information("Start set ticker");
                foreach (var item in profilesList1)
                {
                    VolumeProfile maxVol = item.VolumeProfiles.OrderByDescending(x => (x.VolumeGreen + x.VolumeRed)).FirstOrDefault();
                    //Instrument instrument = await marketDataCollector.GetInstrumentByFigi(item.Figi);
                    decimal volGreenWeight = volumeProfileScreener.RevWeightGreen(maxVol);
                    decimal volRedWeight = 100 - volGreenWeight;

                    Instrument instrument = (from t in instrumentList
                                             where t.Figi == item.Figi
                                             select t).FirstOrDefault();
                    using (StreamWriter sw = new StreamWriter("TickersAll " + candleInterval, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                        sw.WriteLine();
                    }
                    if ((maxVol.UpperBound + maxVol.LowerBound) / 2 < item.Candles.Last().Close)                        
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverPrice " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                            sw.WriteLine();
                        }
                    }
                    if (volGreenWeight > 50)
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverGreenWeght " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                            sw.WriteLine();
                        }
                    }
                    List<AdlResult> adl = Mapper.AdlData(item, item.Candles.Last().Close, 1);
                    var AdlAngle = signal.AdlDegreeAverageAngle(adl, 10, Signal.Adl.Adl);
                    if (AdlAngle > 20 && adl.Last().Adl > 0)
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverADL " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight + " ADL angle = " + AdlAngle + " ADL " + adl.Last().Adl);
                            sw.WriteLine();
                        }
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



        }

    }
}
