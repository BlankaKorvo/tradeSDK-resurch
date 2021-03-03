using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using RetryPolicy;
using Polly;
using Context = Tinkoff.Trading.OpenApi.Network.Context;
namespace TinkoffData
{
    
    public class Market
    {
         //var retry = RetryPolicy.Model.getRetry();
        async Task<CandleList> GetCandleByFigiAsync(Context context, string figi, CandleInterval interval, DateTime to)
        {

            Log.Information("Start GetCandleByFigiAsync method whith figi: " + figi);
            //DateTime to = DateTime.Now;
            DateTime from = to;
            switch (interval)
            {
                case CandleInterval.Minute:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.TwoMinutes:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.ThreeMinutes:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.FiveMinutes:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.TenMinutes:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.QuarterHour:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.HalfHour:
                    from = to.AddDays(-1);
                    break;
                case CandleInterval.Hour:
                    from = to.AddDays(-7);
                    break;
                case CandleInterval.Day:
                    from = to.AddYears(-1);
                    break;
                case CandleInterval.Week:
                    from = to.AddYears(-2);
                    break;
                case CandleInterval.Month:
                    from = to.AddYears(-10);
                    break;
            }
            Log.Information("Time periods for candles with figi: " + figi + " = " + from + " - " + to);

            try
            {
                CandleList candle = await RetryPolicy.Model.Retry().ExecuteAsync(async () => await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.MarketCandlesAsync(figi, from, to, interval)));
                Log.Information("Return " + candle.Candles.Count + " candles by figi: " + figi + " with " + interval + " lenth");
                Log.Information("Stop GetCandleByFigiAsync method whith figi: " + figi);
                return candle;
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                Log.Information(ex.StackTrace);
                Log.Information("Stop GetCandleByFigiAsync method. Return null");
                return null;
            }
        }

        public List<string> FigiFromCandleList(List<CandleList> Stocks)
        {
            List<string> figi = new List<string>();
            foreach (CandleList item in Stocks)
            {
                figi.Add(item.Figi);
            }
            return figi;
        }


        async public Task<CandleList> GetCandlesTinkoffAsync(Context context, string figi, CandleInterval candleInterval, int candlesCount)
        {
            Log.Information("Start GetCandlesTinkoffAsync method. Figi: " + figi);

            Log.Information("CandleInterval: " + candleInterval.ToString());
            Log.Information("CandleCount: " + candlesCount);
            var date = DateTime.Now;
            int iterCount = 0;
            int finalIterCount = 5;
            List<CandlePayload> AllCandlePayloadTemp = new List<CandlePayload>();

            CandlePayloadEqualityComparer CandlePayloadEqC = new CandlePayloadEqualityComparer();

            if (candleInterval == CandleInterval.Minute
                || candleInterval == CandleInterval.TwoMinutes
                || candleInterval == CandleInterval.ThreeMinutes
                || candleInterval == CandleInterval.FiveMinutes
                || candleInterval == CandleInterval.TenMinutes
                || candleInterval == CandleInterval.QuarterHour
                || candleInterval == CandleInterval.HalfHour)
            {
                while (AllCandlePayloadTemp.Count < candlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddDays(-1);
                    iterCount++;
                    if (iterCount > finalIterCount)
                    {
                        Log.Information(figi + " could not get the number of candles needed in " + finalIterCount + " attempts ");
                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
                        return null;
                    }
                }
            }
            else if (candleInterval == CandleInterval.Hour)
                while (AllCandlePayloadTemp.Count < candlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddDays(-7);
                    iterCount++;
                    if (iterCount > finalIterCount)
                    {
                        Log.Information(figi + " could not get the number of candles needed in " + finalIterCount + " attempts ");
                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
                        return null;
                    }
                }
            else if (candleInterval == CandleInterval.Day)
            {
                while (AllCandlePayloadTemp.Count < candlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddYears(-1);
                    iterCount++;
                    if (iterCount > finalIterCount)
                    {
                        Log.Information(figi + " could not get the number of candles needed in " + finalIterCount + " attempts ");
                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
                        return null;
                    }
                }
            }

            List<CandlePayload> candlePayload = (from u in AllCandlePayloadTemp
                                                 orderby u.Time
                                                 select u).ToList();

            CandleList candleList = new CandleList(figi, candleInterval, candlePayload);
            Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return candle list");
            return candleList;
        }

        async Task<List<CandlePayload>> GetUnionCandles(Context context, string figi, CandleInterval candleInterval, DateTime date, List<CandlePayload> AllCandlePayloadTemp, CandlePayloadEqualityComparer CandlePayloadEqC)
        {
            Log.Information("Start GetUnionCandles method with figi: " + figi);
            Log.Information("Count geting candles = " + AllCandlePayloadTemp.Count);
            CandleList candleListTemp = await GetCandleByFigiAsync(context, figi, candleInterval, date);//.GetAwaiter().GetResult();
            Log.Information(candleListTemp.Figi + " GetCandleByFigi: " + candleListTemp.Candles.Count + " candles");
            AllCandlePayloadTemp = AllCandlePayloadTemp.Union(candleListTemp.Candles, CandlePayloadEqC).ToList();
            //AllCandlePayloadTemp = AllCandlePayloadTemp.Union(candleListTemp.Candles, CandlePayloadEqC).ToList();
            Log.Information("GetUnionCandles return: " + AllCandlePayloadTemp.Count + " count candles");
            Log.Information("Stop GetUnionCandles method with figi: " + figi);
            return AllCandlePayloadTemp;
        }
        public async Task<Orderbook> GetOrderbook(Context context, string figi, int depth)
        {

            Orderbook orderbook = await RetryPolicy.Model.Retry().ExecuteAsync(async () => await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.MarketOrderbookAsync(figi, depth)));

            if (orderbook.Asks.Count == 0 || orderbook.Bids.Count == 0)
            {
                Log.Error("Exchange by instrument " + figi + " not working");
                return null;
            }
            Log.Information("Orderbook Figi: " + orderbook.Figi);
            Log.Information("Orderbook Depth: " + orderbook.Depth);
            Log.Information("Orderbook Asks Price: " + orderbook.Asks.FirstOrDefault().Price);
            Log.Information("Orderbook Asks Quantity: " + orderbook.Asks.FirstOrDefault().Quantity);

            Log.Information("Orderbook Bids Price: " + orderbook.Bids.Last().Price);
            Log.Information("Orderbook Bids Quantity: " + orderbook.Bids.Last().Quantity);

            Log.Information("Orderbook ClosePrice: " + orderbook.ClosePrice);
            Log.Information("Orderbook LastPrice: " + orderbook.LastPrice);
            Log.Information("Orderbook LimitDown: " + orderbook.LimitDown);
            Log.Information("Orderbook LimitUp: " + orderbook.LimitUp);
            Log.Information("Orderbook TradeStatus: " + orderbook.TradeStatus);
            Log.Information("Orderbook MinPriceIncrement: " + orderbook.MinPriceIncrement);
            return orderbook;

        }
    }
}
