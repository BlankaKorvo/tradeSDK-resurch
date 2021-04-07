using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using MarketDataModules;
using DataCollector;
//using Tinkoff.Trading.OpenApi.Models;
using TinkoffAdapter.Authority;
using CandleInterval = MarketDataModules.CandleInterval;

namespace ScreenerStocks.Helpers
{
    public class GetStocksHistory
    {
        //GetTinkoffData market = new GetTinkoffData();
        MarketDataCollector dataCollector = new MarketDataCollector();
        internal async Task<List<Instrument>> AllUsdStocksAsync()
        {
            Log.Information("Start AllUsdStocks method");
            List<Instrument> usdStocks = new List<Instrument>();
            InstrumentList stocks = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await dataCollector.GetInstrumentListAsync());
            Log.Information("Get All MarketInstruments. Count =  " + stocks.Instruments.Count);
            foreach (Instrument item in stocks.Instruments)
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

        internal async Task<List<CandlesList>> AllUsdCandlesAsync(CandleInterval candleInterval, int candelCount)
        {
            Log.Information("Start AllUsdCandles method");
            List<Instrument> stocks = await AllUsdStocksAsync();
            Log.Information("Get All MarketInstruments. Count =  " + stocks.Count);
            List<CandlesList> usdCandels = new List<CandlesList>();
            foreach (var item in stocks)
            {
                CandlesList candle = await dataCollector.GetCandlesAsync(item.Figi, candleInterval, candelCount);

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
        internal List<CandlesList> AllValidCandles(List<CandlesList> listCandleLists, decimal price, int minutes)
        {
            Log.Information("Start AllValidCandles method");
            List<CandlesList> validCandleLists = new List<CandlesList> { };
            Log.Information("Count input candles: " + listCandleLists.Count);
            Log.Information("AllValidCandles method. price = " + price);
            Log.Information("AllValidCandles method. notTradeMinutes = " + minutes);
            foreach (CandlesList candleList in listCandleLists)
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
        private bool NotTradeable(CandlesList candleList, int notTradeMinutes)
        {
            Log.Information("Start NotTradeable method. Not trade minutes = " + notTradeMinutes);
            var timeNow = DateTime.Now.ToUniversalTime();
            Log.Information("UTC : "+ timeNow.ToString());
            if (candleList == null)
            {
                Log.Warning("CandleList = null");
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
        private bool LessPrice(CandlesList candleList, decimal margin)
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
