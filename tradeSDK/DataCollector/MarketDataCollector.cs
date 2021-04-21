using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketDataModules;
using Serilog;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffAdapter.Authority;
using TinkoffAdapter.DataHelper;
using CandleInterval = MarketDataModules.CandleInterval;
using Currency = MarketDataModules.Currency;
using InstrumentType = MarketDataModules.InstrumentType;
using Orderbook = MarketDataModules.Orderbook;
using OrderbookEntry = MarketDataModules.OrderbookEntry;
using TradeStatus = MarketDataModules.TradeStatus;

namespace DataCollector
{


    public class MarketDataCollector// : GetTinkoffData // : ICandlesList
    {
        GetTinkoffData getTinkoffData = new GetTinkoffData();

        public async Task<InstrumentList> GetInstrumentListAsync()
        {
            return await TinkoffInstrumentList();
        }

        public async Task<Orderbook> GetOrderbookAsync(string figi, int depth)
        {
            return await TinkoffOrderbook(figi, depth);
        }
        public async Task<List<CandlesList>> GetListCandlesAsync(InstrumentList instrumentList, CandleInterval candleInterval, int candlesCount, Providers providers = Providers.Tinkoff)
        {
            List<CandlesList> listCandlesList = new List<CandlesList>();
            foreach (var item in instrumentList.Instruments)
            {
                CandlesList candlesList = await GetCandlesAsync(item.Figi, candleInterval, candlesCount, providers);
                if (candlesList == null)
                {
                    continue;
                }
                else
                {
                    listCandlesList.Add(candlesList);
                }
            }
            return listCandlesList;
        }
        async Task<CandlesList> TinkoffCandles(string figi, CandleInterval candleInterval, int candlesCount)
        {
            Tinkoff.Trading.OpenApi.Models.CandleInterval interval = (Tinkoff.Trading.OpenApi.Models.CandleInterval)candleInterval;

            CandleList tinkoffCandles = await getTinkoffData.GetCandlesTinkoffAsync(figi, interval, candlesCount);
            if (tinkoffCandles == null)
            {
                return null;
            }
            List<CandleStructure> candles =
                new List<CandleStructure>(tinkoffCandles.Candles.Select(x =>
                    new CandleStructure(x.Open, x.Close, x.High, x.Low, x.Volume, x.Time, (CandleInterval)x.Interval, x.Figi)).Distinct());

            CandlesList candlesList = new CandlesList(tinkoffCandles.Figi, candleInterval, candles);
            return candlesList;
        }

        async Task<InstrumentList> TinkoffInstrumentList()
        {
            MarketInstrumentList tinkoffStocks = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await Auth.Context.MarketStocksAsync());
            InstrumentList stocks = 
                new InstrumentList(tinkoffStocks.Total, tinkoffStocks.Instruments.Select(x => 
                    new Instrument(x.Figi, x.Ticker, x.Isin, x.MinPriceIncrement, x.Lot, (Currency)x.Currency, x.Name, (InstrumentType)x.Type)).ToList());
            return stocks;
        }

        private async Task<Orderbook> TinkoffOrderbook(string figi, int depth)
        {
            Log.Information("Tinkoff. Start  get orderbook");
            Tinkoff.Trading.OpenApi.Models.Orderbook tinOrderbook = await getTinkoffData.GetOrderbookAsync(figi, depth);
            if (tinOrderbook == null)
            {
                return null;
            }
            List<OrderbookEntry> bids = new List<OrderbookEntry>(tinOrderbook.Bids.Select(x => new OrderbookEntry(x.Quantity, x.Price)));
            List<OrderbookEntry> asks = new List<OrderbookEntry>(tinOrderbook.Asks.Select(x => new OrderbookEntry(x.Quantity, x.Price)));
            TradeStatus tradeStatus = (TradeStatus)tinOrderbook.TradeStatus;
            Orderbook orderbook = 
                new Orderbook(tinOrderbook.Depth, bids, asks, tinOrderbook.Figi, tradeStatus, tinOrderbook.MinPriceIncrement, tinOrderbook.FaceValue,
                    tinOrderbook.LastPrice, tinOrderbook.ClosePrice, tinOrderbook.LimitUp, tinOrderbook.LimitDown);
            Log.Information("Tinkoff. Stop  get orderbook");
            return orderbook;
        }

        public async Task<CandlesList> GetCandlesAsync(string figi, CandleInterval candleInterval, int candlesCount, Providers providers = Providers.Tinkoff)
        {
            switch (providers)
            {
                case Providers.Tinkoff:
                    return await TinkoffCandles(figi, candleInterval, candlesCount);
                case Providers.Finam:
                    return null;
            }
            return null;
        }

        public async Task<Instrument> GetInstrumentByFigi(string figi)
        {
            MarketInstrument instrumentT = await getTinkoffData.GetMarketInstrumentByFigi(figi);
            Instrument instrument = new Instrument(instrumentT.Figi, instrumentT.Ticker, 
                instrumentT.Isin, instrumentT.MinPriceIncrement, instrumentT.Lot, 
                (Currency)instrumentT.Currency, instrumentT.Name, (InstrumentType)instrumentT.Type);
            return instrument;
        }



    }
}

