using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using System.Diagnostics;
using Serilog;
using TinkoffAdapter;
using RetryPolicy;
using TinkoffAdapter.DataHelper;
using TinkoffAdapter.Authority;
using DataCollector;
using CandleInterval = MarketDataModules.CandleInterval;
using ScreenerStocks;
using ScreenerStocks.Helpers;
using MarketDataModules;

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
            
            GetTinkoffData market = new GetTinkoffData();
            MarketDataCollector marketDataCollector = new MarketDataCollector();
            GetStocksHistory getStocksHistory = new GetStocksHistory();
            //SandboxContext context = new Auth().GetSanboxContext();
            //Serialization ser = new Serialization();


            //var figi = "BBG000BVPV84"; //AMZN
            //var figi = "BBG0013HGFT4"; //USDRUS
            //var figi = "BBG0018SLC07"; //SQ
            var candleInterval = CandleInterval.FiveMinutes;

            int candlesCount = 45;
            decimal margin = 9000;

            //GetCandlesCollector dataCollector = new GetCandlesCollector();

            //var x = await dataCollector.TinkoffCandles("BBG000BVPV84", candleInterval, 45);
            //foreach (var item in x.Candles)
            //{
            //    Console.WriteLine(item.Open);
            //    Console.WriteLine(item.Time); 
            //}
            //Console.ReadKey();
            //List<string> Tickets = new List<string>() {"qdel", "sage", "bio", "coo", "rgr", "hear", "wor", "msgn"};
            //List<string> Figis = new List<string>();
            
            //foreach (var item in Tickets)
            //{
            //    var sbt = await context.MarketSearchByTickerAsync(item);
            //    Figis.Add(sbt.Instruments.Last().Figi);
            //}


            //List<string> Figis = new List<string>() { "BBG000B9XRY4", "BBG000NS03H7", "BBG000BPH459", "BBG000D8RG11", "BBG0016SSV00", "BBG000BM6N47", "BBG000HL7499" };
            //System config


            // var p = RetryPolicy.Model.Retry();
            //int x = p.ExecuteAsync(() => 2 + 3);


            //while (true)
            //{
            //    foreach (var item in Figis)
            //    {
            //        //var candles = await market.GetCandlesTinkoffAsync(context, item, CandleInterval.FiveMinutes, candlesCount);
            //        Log.Information("Start ScreenerStocks for: " + item);
            //        TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item, CandlesCount = candlesCount, candleInterval = candleInterval, context = context, Margin = margin };
            //        Log.Information("Get object TinkoffTrading with FIGI: " + item);
            //        TransactionModel transactionData = await tinkoffTrading.PurchaseDecisionAsync();
            //        Log.Information("Get TransactionModel: " + transactionData.Figi);
            //        if (transactionData.Operation == TinkoffTrade.Operation.notTrading)
            //        { continue; }
            //        Log.Information("TransactionModel margin = " + transactionData.Margin);
            //        Log.Information("TransactionModel operation = " + transactionData.Operation);
            //        Log.Information("TransactionModel price = " + transactionData.Price);
            //        Log.Information("TransactionModel quantity = " + transactionData.Quantity);


            //        //переписать логику нахрен....


            //        if (transactionData.Operation == TinkoffTrade.Operation.toLong)
            //        {
            //            Log.Information("If transactionData.Operation = " + TinkoffTrade.Operation.toLong.ToString());
            //            Log.Information("Start first transaction");
            //            await tinkoffTrading.TransactionAsync(transactionData);
            //            int i = 2;
            //            do
            //            {
            //                Log.Information("Start " + i + " transaction");
            //                transactionData = await tinkoffTrading.PurchaseDecisionAsync();
            //                await tinkoffTrading.TransactionAsync(transactionData);
            //                i++;
            //            }
            //            while (await market.PresentInPortfolioAsync(context, transactionData.Figi));
            //            Log.Information("Stop ScreenerStocks after trading");
            //        }
            //        else
            //        {
            //            continue;
            //            Log.Information("Stop ScreenerStocks");
            //        }
            //    }
            //}




            MishMashScreener mishMashScreener = new MishMashScreener();

            try
            {
                List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                await mishMashScreener.CycleTrading(candleInterval, candlesCount, margin, instrumentList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }


            //var marInstr = await context.MarketStocksAsync();
            //foreach (var item in marInstr.Instruments)
            //{
            //    Log.Information("Start " + item.Figi);
            //    await context.MarketCandlesAsync(item.Figi, DateTime.Now.AddDays(-20), DateTime.Now, CandleInterval.Day);
            //    Log.Information("Stop " + item.Figi);
            //}


            //while (true)
            //{
            //    try
            //    {
            //        foreach (string item in Figis)
            //        {
            //            again:
            //            try
            //            {
            //                sleep = DynamicSleep(sleep);
            //                Log.Information("Sleep = " + sleep);
            //                Thread.Sleep(sleep);
            //                TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item, candleInterval = candleInterval, context = context, CandleCount = candleCount, Margin = margin };
            //                TransactionModel transactionData = await tinkoffTrading.PurchaseDecision();
            //                tinkoffTrading.Transaction(transactionData);
            //            }
            //            catch (Exception ex)
            //            {
            //                if (ex.Message.Contains("TooManyRequests: Too many requests.."))
            //                {
            //                    sleep += 10;
            //                    Log.Error(ex.ToString());
            //                    Log.Information("Retray with: " + item);
            //                    goto again;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex.ToString());

            //    }
            //    finally
            //    {
            //    }

            //}


        }

        private static int DynamicSleep(int sleep)
        {
            sleep -= 1;
            if (sleep < 0)
            {
                sleep = 0;
            }
            else if (sleep > 1000)
            {
                sleep = 1000;
            }

            return sleep;
        }
    }
}
