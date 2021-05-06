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

        internal bool VolumeLongSignal(CandlesList candleList)
        {
            Log.Information("Start Volume LongSignal. Figi: " + candleList.Figi);
            int count = candleList.Candles.Count;
            decimal volLast = candleList.Candles.Last().Volume;
            decimal volPreLast = candleList.Candles[count-2].Volume;
            Log.Information("volLast: " + volLast + " must be more then volPreLast: " + volPreLast);
            if (volLast > volPreLast)
            {
                Log.Information("VolumeSignal = Long - true for: " + candleList.Figi);
                return true; 
            }
            else
            {
                Log.Information("VolumeSignal = Long - false for: " + candleList.Figi);
                return false; 
            }
        }


    }
}
