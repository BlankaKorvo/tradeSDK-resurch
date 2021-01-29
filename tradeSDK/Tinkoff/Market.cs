using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace Tinkoff
{
    public class Market
    {
        async Task<CandleList> GetCandleByFigi(Context context, string figi, CandleInterval interval, DateTime to)
        {
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
            var candle = await context.MarketCandlesAsync(figi, from, to, interval);
            return candle;
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


        public async Task<CandleList> GetCandlesTinkoff(Context context, string figi, CandleInterval candleInterval, int CandlesCount)
        {
            var date = DateTime.Now;
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
                while (AllCandlePayloadTemp.Count < CandlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddDays(-1);
                }
            }
            else if (candleInterval == CandleInterval.Hour)
                while (AllCandlePayloadTemp.Count < CandlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddDays(-7);
                }
            else if (candleInterval == CandleInterval.Day)
            {
                while (AllCandlePayloadTemp.Count < CandlesCount)
                {
                    AllCandlePayloadTemp = await GetUnionCandles(context, figi, candleInterval, date, AllCandlePayloadTemp, CandlePayloadEqC);
                    date = date.AddYears(-1);
                }
            }

            List<CandlePayload> candlePayload = (from u in AllCandlePayloadTemp
                                                 orderby u.Time
                                                 select u).ToList();

            CandleList candleList = new CandleList(figi, candleInterval, candlePayload);
            return candleList;
        }

        async Task<List<CandlePayload>> GetUnionCandles(Context context, string figi, CandleInterval candleInterval, DateTime date, List<CandlePayload> AllCandlePayloadTemp, CandlePayloadEqualityComparer CandlePayloadEqC)
        {
            CandleList candleListTemp = await GetCandleByFigi(context, figi, candleInterval, date);
            AllCandlePayloadTemp = AllCandlePayloadTemp.Union(candleListTemp.Candles, CandlePayloadEqC).ToList();
            return AllCandlePayloadTemp;
        }
    }
}
