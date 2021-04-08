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

        decimal percent = 0.5M;
        internal bool CandleLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start CandleSignal LongSignal. Figi: " + candleList.Figi);
            
            Log.Information("percent = " + percent);
            var persentHighPrice = ((candleList.Candles.Last().High * 100) / deltaPrice) - 100;
            CandleStructure preLastCandle = candleList.Candles[candleList.Candles.Count - 2];
            CandleStructure lastCandle = candleList.Candles.Last();

            if (
                lastCandle.Open >= preLastCandle.Close
                &&
                lastCandle.Open <= deltaPrice
                &&
                persentHighPrice < percent

               )
            {
                Log.Information("price = " + deltaPrice);
                Log.Information("LastOpen = " + lastCandle.Open);
                Log.Information("preLastClose = " + preLastCandle.Close);
                Log.Information("deltaPrice must be >= Open");
                Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent + " %");
                Log.Information("LastOpen must be >= PreLastClose");
                Log.Information("CandleSignal = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("price = " + deltaPrice);
                Log.Information("LastOpen = " + lastCandle.Open);
                Log.Information("preLastClose = " + preLastCandle.Close);
                Log.Information("deltaPrice must be >= Open");
                Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent + " %");
                Log.Information("LastOpen must be >= PreLastClose");
                Log.Information("CandleSignal = Long - false for: " + candleList.Figi);
                return false;
            }
        }
    }
}
