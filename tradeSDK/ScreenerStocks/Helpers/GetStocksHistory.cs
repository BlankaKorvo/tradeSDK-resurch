using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Helpers;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace ScreenerStocks.Helpers
{
    class GetStocksHistory
    {
        async Task<List<MarketInstrument>> AllUsdStocks(Context context)
        {
            List<MarketInstrument> usdStocks = new List<MarketInstrument>();
            MarketInstrumentList stocks = await context.MarketStocksAsync();
            foreach (MarketInstrument item in stocks.Instruments)
            {
                if (item.Currency == Currency.Usd)
                {
                    usdStocks.Add(item);
                }
            }
            return usdStocks;
        }

        public async Task<List<CandleList>> AllUsdCandles(Context context, CandleInterval candleInterval)
        {
            List<MarketInstrument> stocks = await AllUsdStocks(context);
            List<CandleList> usdCandels = new List<CandleList>();
            int x = 0;
            foreach (var item in stocks)
            {
                DateTime dateTo = DateTime.Now;
                if (candleInterval == CandleInterval.Hour)
                {
                    CandleList first = await TinkoffMarketHelper.GetCandleByFigi(context, item.Figi, candleInterval, dateTo);
                    CandleList second = await TinkoffMarketHelper.GetCandleByFigi(context, item.Figi, candleInterval, dateTo.AddDays(-7));
                    List<CandlePayload> allCandles = first.Candles.Union(second.Candles).ToList();
                    CandleList all = new CandleList(item.Figi, candleInterval, allCandles);
                    usdCandels.Add(all);
                }
                else
                {
                    CandleList candleList = await TinkoffMarketHelper.GetCandleByFigi(context, item.Figi, candleInterval, dateTo);
                    usdCandels.Add(candleList);
                }
                Thread.Sleep(300);
                Console.WriteLine(candleInterval + " " + x++);
            }
            return usdCandels;
        }
    }
}
