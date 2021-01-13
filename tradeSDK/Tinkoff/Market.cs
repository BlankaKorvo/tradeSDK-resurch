using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace Tinkoff
{
    public class Market
    {
        public async Task<CandleList> GetCandleByFigi(Context context, string figi, CandleInterval interval, DateTime to)
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
    }
}
