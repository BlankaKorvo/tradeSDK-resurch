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
            List<CandleList> candleLists = await SortUsdCandles(context, candleInterval, candleCount, margin, notTradeMinuts);
            Log.Information("Get Sort USD candles");
            Log.Information("Start of sorted candleLists");
            Log.Information("Count = " + candleLists.Count);
            string nameOfFile = "stoks " + DateTime.Now;
            using (StreamWriter sw = new StreamWriter(nameOfFile.Replace(":", "_").Replace(".", "_"), true, System.Text.Encoding.Default))
            {
                sw.WriteLine("Count = " + candleLists.Count);
                sw.WriteLine("Margin: " + margin);
                sw.WriteLine("NotTradeMinuts: " + notTradeMinuts);
                foreach (var item in candleLists)
                {
                    sw.WriteLine(item.Figi);
                    Log.Information(item.Figi);
                }
            }
            Log.Information("Stop of sorted candleLists");
            while (true)
            {
                Log.Information("Start Screener Stoks");
                await ScreenerStocks(context, candleInterval, candleCount, margin, candleLists);
            }
        }

        public async Task ScreenerStocks(Context context, CandleInterval candleInterval, int candleCount, decimal margin, List<CandleList> CandleLists)
        {
            foreach (var item in CandleLists)
            {
                Log.Information("Start ScreenerStocks");
                TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item.Figi, CandleCount = candleCount, candleInterval = candleInterval, context = context, Margin = margin };
                Log.Information("Get object TinkoffTrading with FIGI: " + item.Figi);
                TransactionModel transactionData = await tinkoffTrading.PurchaseDecision();
                Log.Information("Get transaction data: "+ transactionData.Figi);
                if (transactionData.Operation == TinkoffTrade.Operation.notTrading)
                { continue; }
                Log.Information("TransactionModel margin = " + transactionData.Margin);
                Log.Information("TransactionModel operation = " + transactionData.Operation);
                Log.Information("TransactionModel price = " + transactionData.Price);
                Log.Information("TransactionModel quantity = " + transactionData.Quantity);


                //переписать логику нахрен....


                if (transactionData.Operation == TinkoffTrade.Operation.toLong)
                {
                    Log.Information("");
                    Log.Information("Start first transaction");
                    tinkoffTrading.Transaction(transactionData);
                    int i = 2;
                    while (transactionData.Operation == TinkoffTrade.Operation.fromLong)
                    {
                        Log.Information("Start " + i + " transaction");
                        transactionData = await tinkoffTrading.PurchaseDecision();
                        tinkoffTrading.Transaction(transactionData);
                        i++;
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
            Log.Information("Start SortUsdCandles. Param: ");
            Log.Information("candleInterval: " + candleInterval);
            Log.Information("candleCount: " + candleCount);
            Log.Information("margin: " + margin);
            Log.Information("notTradeMinuts: " + notTradeMinuts);
            List<CandleList> allUsdCandleLists = await AllUsdCandles(context, candleInterval, candleCount);
            Log.Information("Get All USD candlesLists. Count: " + allUsdCandleLists.Count);
            List<CandleList> validCandleLists = AllValidCandles(allUsdCandleLists, margin, notTradeMinuts);
            Log.Information("Return Valid candlesLists. Count: " + validCandleLists.Count);
            Log.Information("Stop SortUsdCandles");
            return validCandleLists;
        }
    }    
}
