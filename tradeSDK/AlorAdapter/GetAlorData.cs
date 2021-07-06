//using MarketDataModules;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace AlorAdapter
//{
//    public class GetAlorData
//    {
//        public async Task<CandlesList> GetCandlesAlorAsync(string figi, CandleInterval candleInterval, int candlesCount, int attemptsCount = 5)
//        {
//            Log.Information("Start GetCandlesTinkoffAsync method. Figi: " + figi);

//            Log.Information("CandleInterval: " + candleInterval.ToString());
//            Log.Information("CandleCount: " + candlesCount);
//            var date = DateTime.Now;
//            int iterCount = 0;
//            List<CandleStructure> AllCandlePayloadTemp = new List<CandleStructure>();

//            ComparerTinkoffCandlePayloadEquality CandlePayloadEqC = new ComparerTinkoffCandlePayloadEquality();

//            if (candleInterval == CandleInterval.Minute
//                || candleInterval == CandleInterval.TwoMinutes
//                || candleInterval == CandleInterval.ThreeMinutes
//                || candleInterval == CandleInterval.FiveMinutes
//                || candleInterval == CandleInterval.TenMinutes
//                || candleInterval == CandleInterval.QuarterHour
//                || candleInterval == CandleInterval.HalfHour)
//            {
//                while (AllCandlePayloadTemp.Count < candlesCount)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
//                    date = date.AddDays(-1);
//                    iterCount++;
//                    if (iterCount > attemptsCount)
//                    {
//                        Log.Information(figi + " could not get the number of candles needed in " + attemptsCount + " attempts ");
//                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
//                        return null;
//                    }
//                }
//            }
//            else if (candleInterval == CandleInterval.Hour)
//                while (AllCandlePayloadTemp.Count < candlesCount)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
//                    date = date.AddDays(-7);
//                    iterCount++;
//                    if (iterCount > attemptsCount)
//                    {
//                        Log.Information(figi + " could not get the number of candles needed in " + attemptsCount + " attempts ");
//                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
//                        return null;
//                    }
//                }
//            else if (candleInterval == CandleInterval.Day)
//            {
//                while (AllCandlePayloadTemp.Count < candlesCount)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
//                    date = date.AddYears(-1);
//                    iterCount++;
//                    if (iterCount > attemptsCount)
//                    {
//                        Log.Information(figi + " could not get the number of candles needed in " + attemptsCount + " attempts ");
//                        Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
//                        return null;
//                    }
//                }
//            }
//            List<CandlePayload> candlePayloadBefor = (from u in AllCandlePayloadTemp
//                                                      orderby u.Time
//                                                      select u).ToList();

//            int candlesCountNow = candlePayloadBefor.Count();
//            List<CandlePayload> candlePayload = candlePayloadBefor.Skip(candlesCountNow - candlesCount).ToList();

//            CandleList candleList = new CandleList(figi, candleInterval, candlePayload);

//            Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return candle list");
//            return candleList;
//        }

//        public async Task<CandleList> GetCandlesTinkoffAsync(string figi, CandleInterval candleInterval, DateTime dateFrom)
//        {
//            Log.Information("Start GetCandlesTinkoffAsync method. Figi: " + figi);

//            Log.Information("CandleInterval: " + candleInterval.ToString());
//            Log.Information("Date from: " + dateFrom);
//            var dateTo = DateTime.Now;
//            int iterCount = 0;
//            List<CandlePayload> AllCandlePayloadTemp = new List<CandlePayload>();

//            ComparerTinkoffCandlePayloadEquality CandlePayloadEqC = new ComparerTinkoffCandlePayloadEquality();

//            if (candleInterval == CandleInterval.Minute
//                || candleInterval == CandleInterval.TwoMinutes
//                || candleInterval == CandleInterval.ThreeMinutes
//                || candleInterval == CandleInterval.FiveMinutes
//                || candleInterval == CandleInterval.TenMinutes
//                || candleInterval == CandleInterval.QuarterHour
//                || candleInterval == CandleInterval.HalfHour)
//            {
//                while (dateTo.CompareTo(dateFrom) >= 0)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, dateTo, AllCandlePayloadTemp, CandlePayloadEqC);
//                    dateTo = dateTo.AddDays(-1);
//                    Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
//                }
//            }
//            else if (candleInterval == CandleInterval.Hour)
//                while (dateTo.CompareTo(dateFrom) >= 0)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, dateTo, AllCandlePayloadTemp, CandlePayloadEqC);
//                    dateTo = dateTo.AddDays(-7);
//                    //iterCount++;
//                    //if (iterCount > attemptsCount)
//                    //{
//                    //Log.Information(figi + " could not get the number of candles needed in " + attemptsCount + " attempts ");
//                    Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");
//                    //return null;
//                    //}
//                }
//            else if (candleInterval == CandleInterval.Day)
//            {
//                while (dateTo.CompareTo(dateFrom) >= 0)
//                {
//                    AllCandlePayloadTemp = await GetUnionCandlesAsync(figi, candleInterval, dateTo, AllCandlePayloadTemp, CandlePayloadEqC);
//                    dateTo = dateTo.AddYears(-1);
//                    Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return null");

//                }
//            }
//            List<CandlePayload> candlePayloadDis = AllCandlePayloadTemp.Distinct().ToList();

//            List<CandlePayload> candlePayload = (from u in AllCandlePayloadTemp
//                                                 orderby u.Time
//                                                 where u.Time.CompareTo(dateFrom) >= 0
//                                                 select u).ToList();
//            //var candlePayloadR = 

//            CandleList candleList = new CandleList(figi, candleInterval, candlePayload);

//            Log.Information("Stop GetCandlesTinkoffAsync method. Figi: " + figi + ". Return candle list");
//            return candleList;
//        }

//        public async Task<Orderbook> GetOrderbookAsync(string figi, int depth)
//        {
//            Log.Information("Start GetOrderbook method. Figi: " + figi);
//            Orderbook orderbook = await Model.Retry().ExecuteAsync(async () => await Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketOrderbookAsync(figi, depth)));

//            if (orderbook.Asks.Count == 0 || orderbook.Bids.Count == 0)
//            {
//                Log.Warning("Exchange by instrument " + figi + " not working");
//                Log.Information("Stop GetOrderbook method. Figi: " + figi);
//                return null;
//            }
//            Log.Information("Orderbook Figi: " + orderbook.Figi);
//            Log.Information("Orderbook Depth: " + orderbook.Depth);
//            Log.Information("Orderbook Asks Price: " + orderbook.Asks.FirstOrDefault().Price);
//            Log.Information("Orderbook Asks Quantity: " + orderbook.Asks.FirstOrDefault().Quantity);

//            Log.Information("Orderbook Bids Price: " + orderbook.Bids.Last().Price);
//            Log.Information("Orderbook Bids Quantity: " + orderbook.Bids.Last().Quantity);

//            Log.Information("Orderbook ClosePrice: " + orderbook.ClosePrice);
//            Log.Information("Orderbook LastPrice: " + orderbook.LastPrice);
//            Log.Information("Orderbook LimitDown: " + orderbook.LimitDown);
//            Log.Information("Orderbook LimitUp: " + orderbook.LimitUp);
//            Log.Information("Orderbook TradeStatus: " + orderbook.TradeStatus);
//            Log.Information("Orderbook MinPriceIncrement: " + orderbook.MinPriceIncrement);
//            Log.Information("Stop GetOrderbook method. Figi: " + figi);
//            return orderbook;
//        }
//        public async Task<bool> PresentInPortfolioAsync(string figi)
//        {
//            Log.Information("Start PresentInPortfolio method. Figi: " + figi);
//            Portfolio portfolio = await RetryPolicy.Model.Retry().ExecuteAsync(async () => await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await Authority.Auth.Context.PortfolioAsync()));
//            foreach (Portfolio.Position item in portfolio.Positions)
//            {
//                if (item.Figi == figi)
//                {
//                    Log.Information("Stop PresentInPortfolio method. Figi: " + figi + " Return - true");
//                    return true;
//                }
//                else
//                {
//                    continue;
//                }
//            }
//            Log.Information("Stop PresentInPortfolio method. Figi: " + figi + " Return - false");
//            return false;
//        }

//        public List<string> FigiFromCandleList(List<CandleList> Stocks)
//        {
//            List<string> figi = new List<string>();
//            foreach (CandleList item in Stocks)
//            {
//                figi.Add(item.Figi);
//            }
//            return figi;
//        }

//        public async Task<MarketInstrument> GetMarketInstrumentByFigi(string figi)
//        {
//            MarketInstrument instrument = await Model.Retry().ExecuteAsync(async () => await Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketSearchByFigiAsync(figi)));
//            return instrument;
//        }
//        public async Task<MarketInstrumentList> GetMarketInstrumentListByTicker(string ticker)
//        {
//            MarketInstrumentList instruments = await Model.Retry().ExecuteAsync(async () => await Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketSearchByTickerAsync(ticker)));
//            return instruments;
//        }

//        async Task<CandleList> GetOneSetCandlesAsync(string figi, CandleInterval interval, DateTime to)
//        {

//            Log.Information("Start GetCandleByFigiAsync method whith figi: " + figi);
//            //DateTime to = DateTime.Now;
//            to = to.AddMinutes(2);
//            DateTime from = to;
//            switch (interval)
//            {
//                case CandleInterval.Minute:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.TwoMinutes:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.ThreeMinutes:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.FiveMinutes:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.TenMinutes:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.QuarterHour:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.HalfHour:
//                    from = to.AddDays(-1);
//                    break;
//                case CandleInterval.Hour:
//                    from = to.AddDays(-7);
//                    break;
//                case CandleInterval.Day:
//                    from = to.AddYears(-1);
//                    break;
//                case CandleInterval.Week:
//                    from = to.AddYears(-2);
//                    break;
//                case CandleInterval.Month:
//                    from = to.AddYears(-10);
//                    break;
//            }
//            Log.Information("Time periods for candles with figi: " + figi + " = " + from + " - " + to);
//            try
//            {
//                CandleList candle = await Model.Retry().ExecuteAsync(async () => await Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketCandlesAsync(figi, from, to, interval)));
//                Log.Information("Return " + candle.Candles.Count + " candles by figi: " + figi + " with " + interval + " lenth. Date: " + from + " " + to);
//                Log.Information("Stop GetCandleByFigiAsync method whith figi: " + figi);
//                return candle;
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex.Message);
//                Log.Error(ex.StackTrace);
//                Log.Information("Stop GetCandleByFigiAsync method. Return null");
//                return null;
//            }
//        }
//        async Task<List<CandlePayload>> GetUnionCandlesAsync(string figi, CandleInterval candleInterval, DateTime date, List<CandlePayload> AllCandlePayloadTemp, ComparerTinkoffCandlePayloadEquality CandlePayloadEqC)
//        {
//            Log.Information("Start GetUnionCandles. Figi: " + figi);
//            Log.Information("Count geting candles = " + AllCandlePayloadTemp.Count);

//            CandleList candleListTemp = await GetOneSetCandlesAsync(figi, candleInterval, date);
//            Log.Information(candleListTemp.Figi + " GetCandleByFigi: " + candleListTemp.Candles.Count + " candles");

//            AllCandlePayloadTemp = AllCandlePayloadTemp.Union(candleListTemp.Candles, CandlePayloadEqC).ToList();

//            Log.Information("GetUnionCandles return: " + AllCandlePayloadTemp.Count + " count candles");
//            Log.Information("Stop GetUnionCandles. Figi: " + figi);
//            return AllCandlePayloadTemp;
//        }
//    }
//}
//}
