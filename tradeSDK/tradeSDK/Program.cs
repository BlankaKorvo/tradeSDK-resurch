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
using TradingAlgorithms.Algoritms;
using ScreenerStocks;
using TinkoffData;
using TinkoffTrade;
using RetryPolicy;

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
            
            Market market = new Market();
            SandboxContext context = new Auth().GetSanboxContext();
            //Serialization ser = new Serialization();


            //var figi = "BBG000BVPV84"; //AMZN
            //var figi = "BBG0013HGFT4"; //USDRUS
            //var figi = "BBG0018SLC07"; //SQ
            var candleInterval = CandleInterval.FiveMinutes;
            int candleCount = 45;

            decimal margin = 5000;
            List<string> Figis = new List<string>() { "BBG000B9XRY4", "BBG000NS03H7", "BBG000BPH459", "BBG000D8RG11", "BBG0016SSV00", "BBG000BM6N47", "BBG000HL7499" };
            //System config


            // var p = RetryPolicy.Model.Retry();
            //int x = p.ExecuteAsync(() => 2 + 3);



            MishMashScreener mishMashScreener = new MishMashScreener();

            try
            {
                await mishMashScreener.TradeAsync(context, candleInterval, candleCount, margin, 60);
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                Log.Information(ex.StackTrace);
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
