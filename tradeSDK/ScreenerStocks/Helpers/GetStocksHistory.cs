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
        internal async Task<List<MarketInstrument>> AllUsdStocksAsync(Context context)
        {
            Log.Information("Start AllUsdStocks method");
            List<MarketInstrument> usdStocks = new List<MarketInstrument>();
            MarketInstrumentList stocks = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.MarketStocksAsync());
            Log.Information("Get All MarketInstruments. Count =  " + stocks.Instruments.Count);
            foreach (MarketInstrument item in stocks.Instruments)
            {
                if (item.Currency == Currency.Usd)
                {
                    usdStocks.Add(item);
                    Log.Information("Find " + item.Currency.ToString() + " MarketInstrument: " + item.Figi);
                }
                else
                {
                    Log.Information("Find " + item.Currency.ToString() + " MarketInstrument: " + item.Figi);
                    continue; 
                }
            }
            Log.Information("Return  USD MarketInstruments. Count: " + usdStocks.Count);
            Log.Information("Stop AllUsdStocks method");
            return usdStocks;
        }

        internal async Task<List<CandleList>> AllUsdCandlesAsync(Context context, CandleInterval candleInterval, int candelCount)
        {
            Log.Information("Start AllUsdCandles method");
            List<MarketInstrument> stocks = await AllUsdStocksAsync(context);
            Log.Information("Get All MarketInstruments. Count =  " + stocks.Count);
            List<CandleList> usdCandels = new List<CandleList>();
            foreach (var item in stocks)
            {
                CandleList candle = await market.GetCandlesTinkoffAsync(context, item.Figi, candleInterval, candelCount);

                if (candle == null)
                {
                    Log.Information("Candle is null");
                    continue;
                }                
                else if (candle.Candles.Count < candelCount)
                {
                    Log.Information("Get candle with figi: " + candle.Figi);
                    Log.Information(item.Figi + " is shortly then expected candle count");
                    continue;
                }
                else
                {
                    Log.Information("Get candle with figi: " + candle.Figi);
                    usdCandels.Add(candle);                    
                    Log.Information("Add USD Candles with figi: " + item.Figi + " to candlesList");
                }
                

            }
            Log.Information("Return " + usdCandels.Count + " USD candles");
            Log.Information("Stop AllUsdCandles method");
            return usdCandels;
        }
        internal List<CandleList> AllValidCandles(List<CandleList> listCandleLists, decimal price, int minutes)
        {
            Log.Information("Start AllValidCandles method");
            List<CandleList> validCandleLists = new List<CandleList> { };
            Log.Information("Count input candles: " + listCandleLists.Count);
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
                if (LessPrice(candleList, price) && NotTradeable(candleList, minutes))
                {
                    Log.Information(candleList.Figi + " Add candllist to Valid list");
                    validCandleLists.Add(candleList);
                }
                else 
                {
                    Log.Information(candleList.Figi + " Not add to valid candles, because price < margin or this stocks not tradeble last " + minutes + " minutes");
                }
            }
            Log.Information("Return " + validCandleLists.Count + "valid candlelists");
            Log.Information("Stop AllValidCandles method");
            return validCandleLists;
        }
        //определяет: торговалась ли акция последние заданные минуты
        private bool NotTradeable(CandleList candleList, int notTradeMinutes)
        {
            Log.Information("Start NotTradeable method. Not trade minutes = " + notTradeMinutes);
            var timeNow = DateTime.Now.ToUniversalTime();
            Log.Information("UTC : "+ timeNow.ToString());
            if (candleList == null)
            {
                Log.Error("CandleList = null");
                return false;
            } 
            else if (candleList.Candles.Last().Time < timeNow.AddMinutes(-notTradeMinutes))
            {
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString() + " < then " + DateTime.Now.AddMinutes(-notTradeMinutes));
                //Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString());
                Log.Information(candleList.Figi + " not trading last " + notTradeMinutes + " minutes");
                Log.Information("Stop NotTradeable method. Return - false");
                return false;
            }
            else
            {
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString() + " > then " + DateTime.Now.AddMinutes(-notTradeMinutes));
                Log.Information(candleList.Figi + " is trading last " + notTradeMinutes + " minutes");
                Log.Information("Stop NotTradeable method. Return - true");
                return true;
            }
        }
        // определяет: не привышает ли стоимость ации определенную сумму
        private bool LessPrice(CandleList candleList, decimal margin)
        {
            Log.Information("Start LessPrice with figi: " + candleList.Figi);
            Log.Information("LessPrice. Last Close = " + candleList.Candles.Last().Close);
            Log.Information("LessPrice. Margin = " + margin);
            if (candleList.Candles.Last().Close <= margin)
            {
                Log.Information("Stop LessPrice with figi: " + candleList.Figi + " Return - true");
                return true;
            }
            else
            {
                Log.Information("Stop LessPrice with figi: " + candleList.Figi + " Return - false");
                return false;
            }
        }
    }
}
