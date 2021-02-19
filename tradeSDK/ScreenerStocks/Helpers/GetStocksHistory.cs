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
        internal async Task<List<MarketInstrument>> AllUsdStocks(Context context)
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
            Log.Information("Return all USD MarketInstruments");
            return usdStocks;
        }


        internal async Task<List<CandleList>> AllUsdCandles(Context context, CandleInterval candleInterval, int candelCount)
        {
            List<MarketInstrument> stocks = await AllUsdStocks(context);
            Log.Information("Get All USD MarketInstruments");
            List<CandleList> usdCandels = new List<CandleList>();
            foreach (var item in stocks)
            {
                try
                {
                    usdCandels.Add(await market.GetCandlesTinkoff(context, item.Figi, candleInterval, candelCount));
                    Log.Information("Get Candle for: " + item.Figi);
                }
                catch
                { }
            }
            return usdCandels;
        }
        internal List<CandleList> AllValidCandles(Context context, List<CandleList> listCandleLists, decimal price, int minutes)
        {
            List<CandleList> validCandleLists = new List<CandleList> { };
            foreach (CandleList candleList in listCandleLists)
            {                
                if (LessPrice(candleList, price) && notTradable(candleList, minutes))
                { 
                    validCandleLists.Add(candleList); 
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
