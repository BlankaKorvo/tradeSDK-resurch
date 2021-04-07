using MarketDataModules;
using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffData;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace TradingAlgorithms.IndicatorSignals
{
    partial class Signal : IndicatorSignalsHelper
    {
        internal bool CandleLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start CandleSignal LongSignal. Figi: " + candleList.Figi);
            if (deltaPrice == candleList.Candles.Last().Open)
            {
                Log.Information("deltaPrice = Open");
                Log.Information("CandleSignal = Long - true for: " + candleList.Figi);
                return true;
            }
            decimal percent = 10;
            Log.Information("percent = " + percent);
            var persentHighPrice = ((candleList.Candles.Last().High * 100) / deltaPrice) - 100;
            var persentFitilByCandle = ((candleList.Candles.Last().High - deltaPrice) * 100) / (deltaPrice - candleList.Candles.Last().Open);
            if (
                candleList.Candles.Last().Open < deltaPrice
                &&
                persentHighPrice < percent
                &&
                persentFitilByCandle < percent
               )
            {
                Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent);
                Log.Information("persentHighPrice: " + persentFitilByCandle + "must be < " + percent);
                Log.Information("CandleSignal = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent);
                Log.Information("persentHighPrice: " + persentFitilByCandle + "must be < " + percent);
                Log.Information("CandleSignal = Long - false for: " + candleList.Figi);
                return false;
            }
        }
    }
}
