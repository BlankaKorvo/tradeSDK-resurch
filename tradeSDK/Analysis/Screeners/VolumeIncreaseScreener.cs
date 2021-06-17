using DataCollector;
using MarketDataModules;
using MarketDataModules.Models.Candles;
using ScreenerStocks.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace Analysis.Screeners
{
    public class VolumeIncreaseScreener : GetStocksHistory
    {
        //MarketDataCollector dataCollector = new MarketDataCollector();
        IndicatorSignalsHelper indicatorSignalsHelper = new IndicatorSignalsHelper();
        public List<CandlesList> DramIncreased(List<CandlesList> candlesLists, int count, int increaseK)
        {
            Log.Information("Start DramIncreased");
            List<CandlesList> result = new List<CandlesList> { };
            foreach (CandlesList item in candlesLists)
            {
                Log.Information("Start DramIncreased Analisys: " + item.Figi);
                int candlesCount = item.Candles.Count; //кол-во свечей
                List<decimal> lastListVolume = item.Candles.Skip(candlesCount - count - 1).Take(count).Select(x => x.Volume).ToList(); // последовательность объемов в кол-ве "count" свечей, до последней, не включительно
                 decimal avrageVolumeForLast = lastListVolume.AsQueryable().Average(); // среднее арифместическое
                Log.Information("avrageVolumeForLast: " + avrageVolumeForLast);
                List<decimal> preLastListVolume = item.Candles.Skip(candlesCount - count - 2).Take(count).Select(x => x.Volume).ToList(); // последовательность объемов в кол-ве "count" свечей, до предпоследней, не включительно
                decimal avrageVolumeForPreLast = preLastListVolume.AsQueryable().Average();// среднее арифместическое
                Log.Information("avrageVolumeForPreLast: " + avrageVolumeForPreLast);
                Log.Information("item.Candles.Last().Volume: " + item.Candles.Last().Volume);
                Log.Information("item.Candles[candlesCount - 2].Volume: " + item.Candles[candlesCount - 2].Volume);
                decimal differenceLast = item.Candles.Last().Volume / avrageVolumeForLast;
                Log.Information("differenceLast: " + differenceLast);
                decimal differencePreLast = item.Candles[candlesCount - 2].Volume / avrageVolumeForPreLast;
                Log.Information("differencePreLast: " + differencePreLast);
                if (
                    differenceLast > increaseK
                    ||
                    differencePreLast > increaseK
                    )
                {
                    Log.Information("Stop DramIncreased Analisys: " + item.Figi + " Add to List");
                    result.Add(item);
                }
                else
                {
                    Log.Information("Stop DramIncreased Analisys: " + item.Figi + " Not add to List");
                }
               
            }
            Log.Information("Stop DramIncreased");
            return result;
        }
    }
}

