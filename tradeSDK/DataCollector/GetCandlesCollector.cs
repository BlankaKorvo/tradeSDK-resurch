using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataCollector.Models;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffAdapter.Auth;
using TinkoffAdapter.DataHelper;
using CandleInterval = DataCollector.Models.CandleInterval;
using Currency = DataCollector.Models.Currency;

namespace DataCollector
{
    public class GetCandlesCollector// : GetTinkoffData // : ICandlesList
    {
        GetTinkoffData getTinkoffData = new GetTinkoffData();
        async Task<CandlesList> TinkoffCandles(string figi, CandleInterval candleInterval, int candlesCount)
        {
            Tinkoff.Trading.OpenApi.Models.CandleInterval interval = (Tinkoff.Trading.OpenApi.Models.CandleInterval)candleInterval;
            CandleList tinkoffCandles = await getTinkoffData.GetCandlesTinkoffAsync(figi, interval, candlesCount);

            List<CandleStructure> candles = new List<CandleStructure>(tinkoffCandles.Candles.Select(x => new CandleStructure(x.Open, x.Close, x.High, x.Low, x.Volume, x.Time, (CandleInterval)x.Interval, x.Figi)).Distinct());
            CandlesList candlesList = new CandlesList(tinkoffCandles.Figi, candleInterval, candles);
            return candlesList;
        }

        async Task<InstrumentList> TinkoffInstrumentList()
        {
            MarketInstrumentList tinkoffStocks = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketStocksAsync());

            InstrumentList stocks = new InstrumentList(tinkoffStocks.Total, tinkoffStocks.Instruments.Select(x => new Instrument(x.Figi, x.Ticker, x.Isin, x.MinPriceIncrement, x.Lot, x.Currency, x.Name, x.Type)));
            return stocks;
        }

        public async Task<CandlesList> GetCandles(string figi, CandleInterval candleInterval, int candlesCount)
        {
            return await TinkoffCandles(figi, candleInterval, candlesCount);
        }
    }
}

