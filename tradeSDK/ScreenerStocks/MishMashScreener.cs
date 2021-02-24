﻿using ScreenerStocks.Helpers;
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
using Quartz;

namespace ScreenerStocks
{
    public class MishMashScreener : GetStocksHistory
    {
        Market market = new Market();

        public async Task Trade(Context context, CandleInterval candleInterval, int candleCount, decimal margin, int notTradeMinuts)
        {
            var candleLists = await SortUsdCandles(context, candleInterval, candleCount, margin, notTradeMinuts);
            Log.Information("Get Sort USD candles");

            while(true)
            {
                Log.Information("Start Screener Stoks");
                await ScreenerStocks(context, candleInterval, candleCount, margin, candleLists);

            }
        }

        public async Task ScreenerStocks(Context context, CandleInterval candleInterval, int candleCount, decimal margin, List<CandleList> CandleLists)
        {
            foreach (var item in CandleLists)
            {                
                TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item.Figi, CandleCount = candleCount, candleInterval = candleInterval, context = context, Margin = margin };
                Log.Information("Get object TinkoffTrading with FIGI: " + item.Figi);
                TransactionModel transactionData = await tinkoffTrading.PurchaseDecision();
                Log.Information("Get object TransactionModel for Figi: " + item.Figi);
                Log.Information("TransactionModel margin = " + transactionData.Margin);
                Log.Information("TransactionModel operation = " + transactionData.Operation);
                Log.Information("TransactionModel price = " + transactionData.Price);
                Log.Information("TransactionModel quantity = " + transactionData.Quantity);
                if (transactionData.Operation == TinkoffTrade.Operation.toLong)
                {
                    Log.Information("Start first transaction");
                    tinkoffTrading.Transaction(transactionData);
                    while (transactionData.Operation == TinkoffTrade.Operation.fromLong)
                    {
                        transactionData = await tinkoffTrading.PurchaseDecision();
                        tinkoffTrading.Transaction(transactionData);
                    }
                }
                else 
                {
                    continue;
                }
            }
        }

        async Task<List<CandleList>> SortUsdCandles(Context context, CandleInterval candleInterval, int candleCount, decimal margin, int notTradeMinuts)
        {
            List<CandleList> allUsdCandles = await AllUsdCandles(context, candleInterval, candleCount);
            Log.Information("Get All USD candles");
            List<CandleList> CandleLists = AllValidCandles(context, allUsdCandles, margin, notTradeMinuts);
            Log.Information("Get Valid candles");            
            return CandleLists;
        }

        //public async Task Screener(Context context, CandleInterval candleInterval, int candleCount)
        //{
        //    List<MarketInstrument> stocks = await AllUsdStocks(context);
        //    Log.Information("Get All USD MarketInstruments");
        //    int sleep = 0;
        //    foreach (var item in stocks)
        //    {

        //    again:
        //        Thread.Sleep(sleep);
        //        Log.Information("Sleep: " + sleep);
        //        try
        //        {                    
        //            CandleList candleList = await market.GetCandlesTinkoff(context, item.Figi, candleInterval, candleCount);
        //            if (candleList == null)
        //            {
        //                Log.Error("Candle " + item.Figi +" = null");
        //                continue;
        //            }
        //            else if( candleList.Candles.Last().Time < DateTime.Now.ToUniversalTime().AddMinutes(-70) )
        //            {
        //                Log.Information("Last time candle of " + item.Figi + " is " + candleList.Candles.Last().Time.ToString() +" menshe then " + DateTime.Now.AddMinutes(-10));
        //                Log.Information("Last time candle of " + item.Figi + " is " + candleList.Candles.Last().Time.ToString());
        //                Log.Error(item.Figi + " not trading last hour minutes");
        //                continue;
        //            }
        //            Orderbook orderbook = await market.GetOrderbook(context, item.Figi, 1);
        //            if (orderbook == null)
        //            {
        //                Log.Error("OrderBook = null");
        //                continue;
        //            }

        //            decimal deltaPrice = (orderbook.Asks.Last().Price + orderbook.Bids.Last().Price) / 2;
        //            Log.Information("DeltaPrice = " + deltaPrice);
        //            TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item.Figi, candleInterval = candleInterval, countStoks = 3 };
        //            bool present = false;
        //            do
        //            {
        //            againTrade:
        //                try
        //                {
        //                    await tinkoffTrading.PurchaseDecision();
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error(ex.ToString());
        //                    if (ex.Message.Contains("TooManyRequests: Too many requests.."))
        //                    {
        //                        Log.Error(ex.ToString());
        //                        Log.Information("Retray PurchaseDecision with: " + item.Figi);
        //                        Thread.Sleep(sleep);
        //                        goto againTrade;
        //                    }
        //                }
        //                var portfolio = await context.PortfolioAsync();
        //                foreach (var itemP in portfolio.Positions)
        //                {
        //                    if (itemP.Figi == item.Figi)
        //                    {
        //                        present = true;
        //                    }
        //                }
        //            }
        //            while (present);
        //            //Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };
        //            //if (mishmash.Long())
        //            //{
        //            //    using (StreamWriter sw = new StreamWriter("Figi_Long", true, System.Text.Encoding.Default))
        //            //    {
        //            //        sw.WriteLine(DateTime.Now + " " + item.Figi);
        //            //    }
        //            //}
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex.ToString());
        //            if (ex.Message.Contains("TooManyRequests: Too many requests.."))
        //            {
        //                sleep += 10;
        //                Log.Error(ex.ToString());
        //                Log.Information("Retray Screener with: " + item.Figi);
        //                goto again;
        //            }
        //            else
        //            {
        //                Log.Error(ex.ToString());
        //                break;                       
        //            }
        //        }
        //        finally
        //        {

        //        }                
        //        sleep -= 1;
        //        if (sleep < 0)
        //        {
        //            sleep = 0;
        //        }
        //        else if (sleep > 1000)
        //        {
        //            sleep = 1000;
        //        }

        //    }
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
