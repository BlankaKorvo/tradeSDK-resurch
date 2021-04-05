using ScreenerStocks.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffAdapter;
using TinkoffAdapter.DataHelper;
using TinkoffAdapter.TinkoffTrade;

namespace ScreenerStocks
{
    public class MishMashScreener : GetStocksHistory
    {
        GetTinkoffData getTinkoffData = new GetTinkoffData();
        public async Task Screener(Context context, CandleInterval candleInterval, int candleCount, decimal margin, int notTradeMinuts)
        {
            Log.Information("Start Trade method:");
            Log.Information("candleInterval:" + candleInterval);
            Log.Information("candleCount:" + candleInterval);
            Log.Information("margin:" + margin);
            Log.Information("notTradeMinuts:" + notTradeMinuts);
            List<CandleList> candleLists = await SortUsdCandlesAsync(context, candleInterval, candleCount, margin, notTradeMinuts);
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
                await Trading(context, candleInterval, candleCount, margin, candleLists);
            }
        }

        public async Task Trading(Context context, CandleInterval candleInterval, int candleCount, decimal margin, List<CandleList> CandleLists)
        {
            Log.Information("Start ScreenerStocks: ");
            Log.Information("Count instruments =  " + CandleLists.Count);
            Log.Information("candleCount =  " + candleCount);
            Log.Information("margin =  " + margin);
            foreach (var item in CandleLists)
            {
                Log.Information("Start ScreenerStocks for: " + item.Figi);
                TinkoffTrading tinkoffTrading = new TinkoffTrading() { Figi = item.Figi, CandlesCount = candleCount, candleInterval = candleInterval, context = context, Purchase = margin };
                Log.Information("Get object TinkoffTrading with FIGI: " + item.Figi);
                TransactionModel transactionData = await tinkoffTrading.PurchaseDecisionAsync();
                Log.Information("Get TransactionModel: " + transactionData.Figi);
                if (transactionData.Operation == TinkoffAdapter.Operation.notTrading)
                { continue; }
                Log.Information("TransactionModel margin = " + transactionData.Purchase);
                Log.Information("TransactionModel operation = " + transactionData.Operation);
                Log.Information("TransactionModel price = " + transactionData.Price);
                Log.Information("TransactionModel quantity = " + transactionData.Quantity);


                //переписать логику нахрен....


                if (transactionData.Operation == TinkoffAdapter.Operation.toLong)
                {
                    Log.Information("If transactionData.Operation = " + TinkoffAdapter.Operation.toLong.ToString());
                    Log.Information("Start first transaction");
                    await tinkoffTrading.TransactionAsync(transactionData);
                    int i = 2;
                    do
                    {
                        Log.Information("Start " + i + " transaction");
                        transactionData = await tinkoffTrading.PurchaseDecisionAsync();
                        await tinkoffTrading.TransactionAsync(transactionData);
                        i++;
                    }
                    while (await getTinkoffData.PresentInPortfolioAsync(context, transactionData.Figi));
                    Log.Information("Stop ScreenerStocks after trading");
                }
                else
                {
                    Log.Information("Stop ScreenerStocks");
                    continue;
                }
            }
        }
        async Task<List<CandleList>> SortUsdCandlesAsync(Context context, CandleInterval candleInterval, int candleCount, decimal margin, int notTradeMinuts)
        {
            Log.Information("Start SortUsdCandles. Param: ");
            Log.Information("candleInterval: " + candleInterval);
            Log.Information("candleCount: " + candleCount);
            Log.Information("margin: " + margin);
            Log.Information("notTradeMinuts: " + notTradeMinuts);
            List<CandleList> allUsdCandleLists = await AllUsdCandlesAsync(context, candleInterval, candleCount);
            Log.Information("Get All USD candlesLists. Count: " + allUsdCandleLists.Count);
            List<CandleList> validCandleLists = AllValidCandles(allUsdCandleLists, margin, notTradeMinuts);
            Log.Information("Return Valid candlesLists. Count: " + validCandleLists.Count);
            Log.Information("Stop SortUsdCandles");
            return validCandleLists;
        }
    }    
}
