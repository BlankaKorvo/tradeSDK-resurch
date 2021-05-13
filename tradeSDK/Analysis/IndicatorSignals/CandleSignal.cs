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

        decimal percent = 1M;
        decimal deltaPriceDifference = 2; //во столько раз, текущая свеча может быть больше прошлой
        internal bool CandleLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start CandleSignal LongSignal. Figi: " + candleList.Figi);
            int greenCountCandles = 2;
            Log.Information("percent = " + percent);
   

            CandleStructure preLastCandle = candleList.Candles[candleList.Candles.Count - 2];
            CandleStructure lastCandle = candleList.Candles.LastOrDefault();


            decimal persentHighPrice = ((lastCandle.High * 100) / deltaPrice) - 100;
            decimal lastHighPrice = ((preLastCandle.High * 100) / deltaPrice) - 100;


            decimal deltaPricePreLastCandle = 0;
            decimal deltaPriceLastCandle = 0;
            if (candleList.Interval == CandleInterval.Minute) //эспешиали фор ебучий тинькофф, которые не отрисовывает текущую свечку. Поэтому open - это close last candle
            {
                deltaPricePreLastCandle = lastCandle.High - lastCandle.Low;
                if (deltaPricePreLastCandle == 0)
                {
                    deltaPricePreLastCandle = 0.01m;
                }
                deltaPriceLastCandle = deltaPrice - lastCandle.Close; //эспешиали фор ебучий тинькофф, которые не отрисовывает текущую свечку. Поэтому open - это close last candle
            }
            else
            {
                deltaPricePreLastCandle = preLastCandle.High - preLastCandle.Low;
                if (deltaPricePreLastCandle == 0)
                {
                    deltaPricePreLastCandle = 0.01m;
                }
                deltaPriceLastCandle = deltaPrice - lastCandle.Open; 

            }
            //int countGreenCounts = CountGreenCandles(candleList);
            decimal diff = deltaPriceLastCandle / deltaPricePreLastCandle;

            Log.Information("deltaPrice = " + deltaPrice);
            Log.Information("Interval = " + candleList.Interval);
            Log.Information("LastCandle. Date: " + lastCandle.Time + " Low: " + lastCandle.Low + " Open: " + lastCandle.Open + " Close: " + lastCandle.Close + " High: " + lastCandle.High + " Volume: " + lastCandle.Volume);
            Log.Information("PreLastCandle. Date: " + preLastCandle.Time + " Low: " + preLastCandle.Low + " Open: " + preLastCandle.Open + " Close: " + preLastCandle.Close + " High: " + preLastCandle.High + " Volume: " + lastCandle.Volume);
            Log.Information("Last candle must be green");
            Log.Information("persentHighPrice: " + persentHighPrice + "must be < " + percent + " %");
            Log.Information("lastHighPrice: " + lastHighPrice + "must be < " + percent + " %");
            Log.Information("deltaPriceLastCandle / deltaPricePreLastCandle: " + diff + " must be < " + deltaPriceDifference);

            if (
                //lastCandle.Open >= preLastCandle.Close
                //&&                
                IsCandleGreen(candleList, deltaPrice)
                &&
                persentHighPrice < percent
                &&
                lastHighPrice < percent
                &&
                diff < deltaPriceDifference

               //&&
               //countGreenCounts <= greenCountCandles
               )
            {
                Log.Information("CandleSignal = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("CandleSignal = Long - false for: " + candleList.Figi);
                return false;
            }
        }

        private static bool IsCandleGreen(CandlesList candleList, decimal deltaPrice)
        {
            if (candleList.Interval == CandleInterval.Minute)
            {
                return candleList.Candles.LastOrDefault().Close <= deltaPrice;
            }
            else
            {
                return (candleList.Candles.LastOrDefault().Open <= deltaPrice && candleList.Candles.LastOrDefault().Open <= candleList.Candles.LastOrDefault().Close);
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
