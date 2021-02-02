using ScreenerStocks.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffData;
using TinkoffTrade;
using TradingAlgorithms.Algoritms;

namespace ScreenerStocks
{
    public class MishMashScreener : GetStocksHistory
    {
        Market market = new Market();
        public async Task Screener(Context context, CandleInterval candleInterval, int candleCount)
        {
            List<MarketInstrument> stocks = await AllUsdStocks(context);
            Log.Information("Get All USD MarketInstruments");
            int sleep = 0;
            foreach (var item in stocks)
            {

            again:
                Thread.Sleep(sleep);
                Log.Information("Sleep: " + sleep);
                try
                {                    
                    CandleList candleList = await market.GetCandlesTinkoff(context, item.Figi, candleInterval, candleCount);
                    if (candleList == null)
                    {
                        Log.Error("Candle " + item.Figi +" = null");
                        continue;
                    }
                    else if( candleList.Candles.Last().Time < DateTime.Now.ToUniversalTime().AddMinutes(-10) )
                    {
                        Log.Information("Last time candle of " + item.Figi + " is " + candleList.Candles.Last().Time.ToString() +" menshe then " + DateTime.Now.AddMinutes(-10));
                        Log.Information("Last time candle of " + item.Figi + " is " + candleList.Candles.Last().Time.ToString());
                        Log.Error(item.Figi + " not trading last 10 minutes");
                        continue;
                    }
                    Orderbook orderbook = await market.GetOrderbook(context, item.Figi, 1);
                    if (orderbook == null)
                    {
                        Log.Error("OrderBook = null");
                        continue;
                    }

                    decimal deltaPrice = (orderbook.Asks.Last().Price + orderbook.Bids.Last().Price) / 2;
                    Log.Information("DeltaPrice = " + deltaPrice);
                    TinkoffTrading tinkoffTrading = new TinkoffTrading() { figi = item.Figi, candleInterval = candleInterval, countStoks = 3 };
                    bool present = false;
                    do
                    {
                    againTrade:
                        try
                        {
                            await tinkoffTrading.PurchaseDecision();
                        }
                        catch (Exception ex)
                        {
                            Log.Information(ex.ToString());
                            if (ex.Message.Contains("TooManyRequests: Too many requests.."))
                            {
                                Log.Error(ex.ToString());
                                Log.Information("Retray PurchaseDecision with: " + item.Figi);
                                Thread.Sleep(sleep);
                                goto againTrade;
                            }
                        }
                        var portfolio = await context.PortfolioAsync();
                        foreach (var itemP in portfolio.Positions)
                        {
                            if (itemP.Figi == item.Figi)
                            {
                                present = true;
                            }
                        }
                    }
                    while (present);
                    //Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };
                    //if (mishmash.Long())
                    //{
                    //    using (StreamWriter sw = new StreamWriter("Figi_Long", true, System.Text.Encoding.Default))
                    //    {
                    //        sw.WriteLine(DateTime.Now + " " + item.Figi);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    Log.Information(ex.ToString());
                    if (ex.Message.Contains("TooManyRequests: Too many requests.."))
                    {
                        sleep += 10;
                        Log.Error(ex.ToString());
                        Log.Information("Retray Screener with: " + item.Figi);
                        goto again;
                    }
                    else
                    {
                        Log.Error(ex.ToString());
                        break;                       
                    }
                }
                finally
                {
                    
                }                
                sleep -= 1;
                if (sleep < 0)
                {
                    sleep = 0;
                }
                else if (sleep > 1000)
                {
                    sleep = 1000;
                }

            }
            //List<CandleList> stocks = await AllUsdCandles(context, candleInterval, candleCount);
            //Log.Information("Get All USD Candles");
            //foreach (var item in stocks)
            //{
            //    Orderbook orderbook = await market.GetOrderbook(context, item.Figi, 1);
            //    decimal deltaPrice = (orderbook.Asks.Last().Price + orderbook.Bids.Last().Price) / 2;
            //    Log.Information("DeltaPrice = " + deltaPrice);
            //    Mishmash mishmash = new Mishmash() { candleList = item, deltaPrice  = deltaPrice};
            //    if (mishmash.Long())
            //    {
            //        Console.WriteLine(item.Figi);
            //    }
            //}
        }
    }
}
