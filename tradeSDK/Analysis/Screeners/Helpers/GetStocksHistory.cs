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
        public async Task<List<Instrument>> AllUsdStocksAsync()
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
        internal List<CandlesList> AllValidCandles(List<CandlesList> listCandleLists, decimal price, int notTradeMinutes)
        {
            Log.Information("Start AllValidCandles method");
            List<CandlesList> validCandleLists = new List<CandlesList> { };
            Log.Information("Count input candles: " + listCandleLists.Count);
            Log.Information("AllValidCandles method. price = " + price);
            Log.Information("AllValidCandles method. notTradeMinutes = " + notTradeMinutes);
            foreach (CandlesList candleList in listCandleLists)
            {
                if (ValidCandles(candleList, price, notTradeMinutes))
                {
                    Log.Information(candleList.Figi + " Add candllist to Valid list");
                    validCandleLists.Add(candleList);
                }
            }
            Log.Information("Return " + validCandleLists.Count + "valid candlelists");
            Log.Information("Stop AllValidCandles method");
            return validCandleLists;
        }
        //определяет: торговалась ли акция последние заданные минуты
        private bool NotTradeableCountMinutes(CandlesList candleList, int notTradeMinutes)
        {
            Log.Information("Start NotTradeableCountMinutes method. Not trade minutes = " + notTradeMinutes);
            var timeNow = DateTime.Now.ToUniversalTime();
            Log.Information("UTC : "+ timeNow.ToString());
            Log.Information("Last candle time is : " + candleList.Candles.LastOrDefault().Time + " Close: " + candleList.Candles.LastOrDefault().Close + " Open: " + candleList.Candles.LastOrDefault().Open + " Volume: " +candleList.Candles.LastOrDefault().Volume);
            Log.Information("Last candle time is : " + candleList.Candles[candleList.Candles.Count - 2].Time + " Close: " + candleList.Candles[candleList.Candles.Count - 2].Close + " Open: " + candleList.Candles[candleList.Candles.Count - 2].Open + " Volume: " + candleList.Candles[candleList.Candles.Count - 2].Volume);
            if (candleList == null)
            {
                Log.Warning("CandleList = null");
                return false;
            } 
            else if (candleList.Candles.Last().Time <= timeNow.AddMinutes(-notTradeMinutes))
            {
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString() + " < then " + timeNow.AddMinutes(-notTradeMinutes));
                //Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString());
                Log.Information(candleList.Figi + " not trading last " + notTradeMinutes + " minutes");
                Log.Information("Stop NotTradeable method. Return - false");
                return false;
            }
            else
            {
                Log.Information("Last time candle of " + candleList.Figi + " is " + candleList.Candles.Last().Time.ToString() + " > then " + timeNow.AddMinutes(-notTradeMinutes));
                Log.Information(candleList.Figi + " is trading last " + notTradeMinutes + " minutes");
                Log.Information("Stop NotTradeable method. Return - true");
                return true;
            }
        }

        private bool NotTradeableCountCandles(CandlesList candleList, int notTradeCandles = 3)
        {
            Log.Information("Start NotTradeableCountCandles method. Not trade candles = " + notTradeCandles);
            int notTradeMinutes = 0;
            if (candleList == null)
            {
                Log.Warning("CandleList = null");
                return false;
            }
            switch (candleList.Interval)
            {
                case CandleInterval.Minute:
                    notTradeMinutes = 1 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.TwoMinutes:
                    notTradeMinutes = 2 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.ThreeMinutes:
                    notTradeMinutes = 3 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.FiveMinutes:
                    notTradeMinutes = 5 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.TenMinutes:
                    notTradeMinutes = 10 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.QuarterHour:
                    notTradeMinutes = 15 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.HalfHour:
                    notTradeMinutes = 30 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.Hour:
                    notTradeMinutes = 60 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.Day:
                    notTradeMinutes = 1440 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.Week:
                    notTradeMinutes = 10080 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
                case CandleInterval.Month:
                    notTradeMinutes = 43200 * notTradeCandles;
                    Log.Information("notTradeMinutes = " + notTradeMinutes);
                    break;
            }
            return NotTradeableCountMinutes(candleList, notTradeMinutes);
        }

        // определяет: не привышает ли стоимость ации определенную сумму
        private bool LessPrice(CandlesList candleList, decimal margin)  //проверяем, что цена ниже располагаемой для покупки суммы
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
        internal bool ValidCandles(CandlesList candlesList, decimal price, int notTradeMinutes)
        {
            Log.Information("Start validation CandlesList " + candlesList.Figi);
            if (candlesList == null)
            {
                Log.Information("Nullable candlelist");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return false;
            }
            Log.Information("AllValidCandles method. Candle.Figi: " + candlesList.Figi);
            Log.Information("AllValidCandles method. Candle.Interval: " + candlesList.Interval);
            Log.Information("AllValidCandles method. Candle Count: " + candlesList.Candles.Count);
            if (LessPrice(candlesList, price) && NotTradeableCountMinutes(candlesList, notTradeMinutes))
            {
                Log.Information(candlesList.Figi + " Valid CandlesList");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return true;
            }
            else
            {
                Log.Information(candlesList.Figi + "  Not Valid CandlesList");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return false;
            }
        }
        internal bool ValidCandles(CandlesList candlesList, decimal price)
        {
            Log.Information("Start validation CandlesList " + candlesList.Figi);
            if (candlesList == null)
            {
                Log.Information("Nullable candlelist");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return false;
            }
            Log.Information("AllValidCandles method. Candle.Figi: " + candlesList.Figi);
            Log.Information("AllValidCandles method. Candle.Interval: " + candlesList.Interval);
            Log.Information("AllValidCandles method. Candle Count: " + candlesList.Candles.Count);
            if (LessPrice(candlesList, price) && NotTradeableCountCandles(candlesList))
            {
                Log.Information(candlesList.Figi + " Valid CandlesList");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return true;
            }
            else
            {
                Log.Information(candlesList.Figi + " Not Valid CandlesList");
                Log.Information("Stop validation CandlesList " + candlesList.Figi);
                return false;
            }
        }
    }
}
