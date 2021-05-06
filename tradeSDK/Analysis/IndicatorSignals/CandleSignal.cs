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
    public partial class Signal : IndicatorSignalsHelper
    {

        decimal percent = 0.3M;
        internal bool CandleLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start CandleSignal LongSignal. Figi: " + candleList.Figi);
            int greenCountCandles = 2;
            Log.Information("percent = " + percent);
            decimal persentHighPrice = ((candleList.Candles.Last().High * 100) / deltaPrice) - 100;
            CandleStructure preLastCandle = candleList.Candles[candleList.Candles.Count - 2];
            CandleStructure lastCandle = candleList.Candles.Last();
            int countGreenCounts = CountGreenCandles(candleList);
            //Log.Information("countGreenCounts = " + countGreenCounts + " It must be <= " + greenCountCandles);

            if (
                //lastCandle.Open >= preLastCandle.Close
                //&&
                lastCandle.Open <= candleList.Candles.Last().Close
                &&
                persentHighPrice < percent
                //&&
                //countGreenCounts <= greenCountCandles
               )
            {
                Log.Information("deltaPrice = " + deltaPrice);
                Log.Information("LastOpen = " + lastCandle.Open);
                //Log.Information("preLastClose = " + preLastCandle.Close);
                Log.Information("deltaPrice must be >= Open");
                Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent + " %");
                //Log.Information("LastOpen must be >= PreLastClose");
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
        int CountGreenCandles(CandlesList candlesList)
        {
            int greenCount = 0;
            int candlesCount = candlesList.Candles.Count() - 1;
            for (int i = 0; i <= candlesCount; i++)
            {
                CandleStructure candleStructure = candlesList.Candles[candlesCount - i];
                if (GreenCandle(candleStructure))
                {
                    greenCount++;
                }
                else
                {
                    break;
                }
            }
            return greenCount;
        }
    }
}
