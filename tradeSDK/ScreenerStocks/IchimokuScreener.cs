using ScreenerStocks.Helpers;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Helpers;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TradingAlgorithms;

namespace ScreenerStocks
{
    class IchimokuScreener : GetStocksHistory
    {
        public List<CandleList> IchimokuTradeCandles(List<CandleList> candleLists, bool longTrade)
        {
            List<CandleList> candles = new List<CandleList>();
            ByIchimoku byIchimoku = new ByIchimoku();
            foreach (var candleList in candleLists)
            {
                try
                {
                    List<IchimokuResult> ichimokuResult = byIchimoku.IchimokuDate(candleList);

                    IchimokuResult currentIchResult = ichimokuResult.Last();
                    IchimokuResult preCurrentIchResult = ichimokuResult[ichimokuResult.Count - 2];

                    CandlePayload currentCandle = candleList.Candles.Last();
                    CandlePayload preCurrentCandle = candleList.Candles[ichimokuResult.Count - 2];
                    if (longTrade == true)
                    {
                        if (byIchimoku.LongClassicalConditionForScreener(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle))
                        {
                            candles.Add(candleList);
                        }
                    }
                    else
                    {
                        if (byIchimoku.ShortClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle))
                        {
                            candles.Add(candleList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally { }
            }
            Console.WriteLine("start long " + longTrade);
            foreach (var item in candles)
            {

                Console.WriteLine(item.Figi);

            }
            Console.WriteLine("end long " + longTrade);
            return candles;
        }

        public async Task<List<CandleList>> UsdIchimokuFigi(Context context, bool longTrade, CandleInterval candleInterval)
        {
            List<CandleList> usdStocks = await AllUsdCandles(context, candleInterval);
            return IchimokuTradeCandles(usdStocks, longTrade);
        }

        public async Task<List<CandleList>> UsdIchimokuFigi(Context context, bool longTrade, CandleInterval candleInterval1, CandleInterval candleInterval2)
        {
            List<CandleList> usdStocks1 = await AllUsdCandles(context, candleInterval1);
            List<CandleList> tradeStocks1 = IchimokuTradeCandles(usdStocks1, longTrade);
            List<CandleList> usdStocks2 = await AllUsdCandles(context, candleInterval2);
            List<CandleList> tradeStocks2 = IchimokuTradeCandles(usdStocks2, longTrade);
            return tradeStocks1.Intersect(tradeStocks2).ToList();
        }

        List<CandleList> FillStocksForTrade(List<CandleList> candleLists, decimal SportfolioSum)
        {
            List<CandleList> newList = new List<CandleList>();
            List<CandleList> OrderList = candleLists.OrderBy(c => c.Candles.Last().Close).ToList();
            decimal sum = 0;
            foreach (var item in OrderList)
            {
                if ((sum + item.Candles.Last().Close) <= SportfolioSum)
                {
                    newList.Add(item);
                    sum += item.Candles.Last().Close;
                }
                else
                {
                    break;
                }
            }
            return newList;
        }

        decimal SumPriceStocks(List<CandleList> candleLists)
        {
            decimal sum = 0;
            foreach (var item in candleLists)
            {
                sum += item.Candles.Last().Close;
            }
            return sum;
        }

        List<CandleList> portfolioForTrading(List<CandleList> candleLists, decimal SportfolioSum)
        {
            List<CandleList> currentSet = FillStocksForTrade(candleLists, SportfolioSum);
            decimal razn = SportfolioSum - SumPriceStocks(currentSet);
            while (candleLists.FirstOrDefault().Candles.Last().Close <= razn)
            {
                FillStocksForTrade(currentSet, SportfolioSum);
            }
            return null;
        }
    }
}
