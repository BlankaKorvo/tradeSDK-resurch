using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffData;


namespace ScreenerStocks.Helpers
{
    public class GetStocksHistory
    {
        Market market = new Market();
        async internal Task<List<MarketInstrument>> AllUsdStocks(Context context)
        {
            List<MarketInstrument> usdStocks = new List<MarketInstrument>();
            MarketInstrumentList stocks = await context.MarketStocksAsync();
            Log.Information("Get All MarketInstruments ");
            foreach (MarketInstrument item in stocks.Instruments)
            {
                if (item.Currency == Currency.Usd)
                {
                    usdStocks.Add(item);
                    Log.Information("Find USD MarketInstrument: " + item.Figi);
                }
            }
            Log.Information("Return " + usdStocks.Count + " USD MarketInstruments");
            return usdStocks;
        }

        internal async Task<List<CandleList>> AllUsdCandles(Context context, CandleInterval candleInterval, int candelCount)
        {
            List<MarketInstrument> stocks = await AllUsdStocks(context);
            Log.Information("Get " + stocks.Count + " USD MarketInstruments");
            List<CandleList> usdCandels = new List<CandleList>();
            foreach (var item in stocks)
            {
                try
                {
                    CandleList candle = await market.GetCandlesTinkoff(context, item.Figi, candleInterval, candelCount);
                    if (candle == null)
                    {
                        Log.Information("Candle is null");
                        continue;
                    }
                    else if (candle.Candles.Count < candelCount)
                    {
                        Log.Information(item.Figi + " is shortly then candle count");
                        continue;
                    }
                    else
                    {
                        usdCandels.Add(candle);
                        Log.Information("Get Candle for: " + item.Figi);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            Log.Information("Return " + usdCandels.Count + " USD candles");
            return usdCandels;
        }
        internal List<CandleList> AllValidCandles(List<CandleList> listCandleLists, decimal price, int minutes)
        {
            List<CandleList> validCandleLists = new List<CandleList> { };
            Log.Information("Start AllValidCandles method");
            Log.Information("AllValidCandles method. price = " + price);
            Log.Information("AllValidCandles method. notTradeMinutes = " + minutes);
            foreach (CandleList candleList in listCandleLists)
            {
                if (candleList == null)
                {
                    Log.Information("Nullable candlelist");
                    continue; 
                }
                Log.Information("AllValidCandles method. Candle.Figi: " + candleList.Figi);
                Log.Information("AllValidCandles method. Candle.Interval: " + candleList.Interval);
                Log.Information("AllValidCandles method. Candle Count: " + candleList.Candles.Count);
                if (LessPrice(candleList, price) && notTradable(candleList, minutes))
                {
                    Log.Information(candleList.Figi + " Add to Valid candles");
                    validCandleLists.Add(candleList);
                }
                else 
                {
                    Log.Information(candleList.Figi + " Not Add to Valid candles");
                }
            }
            return validCandleLists;
        }

        private bool notTradable(CandleList candleList, int candelCount)
        {
            var timeNow = DateTime.Now.ToUniversalTime();
            if (candleList == null)
            {
                Log.Error("Candle " + candleList.Figi + " = null");
                return false;
            } 
            else if (candleList.Candles.Last().Time < timeNow.AddMinutes(-candelCount))
            {
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString() + " menshe then " + DateTime.Now.AddMinutes(-candelCount));
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString());
                Log.Error(candleList.Figi + " not trading last " + candelCount + " minutes");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool LessPrice(CandleList candleList, decimal price)
        {
            Log.Information("Start LessPrice with figi: " + candleList.Figi);
            Log.Information("LessPrice. Last Close = " + candleList.Candles.Last().Close);
            if (candleList.Candles.Last().Close <= price)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
