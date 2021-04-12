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
        int stochLookbackPeriod = 14;
        int stochSignalPeriod = 3;
        int stochSmoothPeriod = 1;
        int stochanglesCount = 1;

        internal bool StochLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Stoch LongSignal. Figi: " + candleList.Figi);
            List<StochResult> stoch = Mapper.StochData(candleList, deltaPrice, stochLookbackPeriod, stochSignalPeriod, stochSmoothPeriod);
            Log.Information("Oscillator (%K) = " + stoch.Last().Oscillator);
            Log.Information("Signal (%D) = " + stoch.Last().Signal);
            Log.Information("PercentJ = " + stoch.Last().PercentJ);
            Log.Information("Oscillator (%K) = " + stoch.Last().Oscillator);
            Log.Information("Signal (%D) = " + stoch.Last().Signal);
            Log.Information("PercentJ = " + stoch.Last().PercentJ);

            return true;
        }
        double StochDegreeAverageAngle(List<StochResult> AdlValue, int anglesCount, Stoch line)
        {
            if (line == Stoch.Oscillator)
            {
                List<decimal?> values = AdlValue.Select(na => (decimal?)na.Oscillator).ToList();
                return DeltaDegreeAngle(values, anglesCount);
            }
            else if (line == Stoch.Signal)
            {
                List<decimal?> values = AdlValue.Select(na => (decimal?)na.Signal).ToList();
                return DeltaDegreeAngle(values, anglesCount);
            }
            else 
            {
                List<decimal?> values = AdlValue.Select(na => (decimal?)na.PercentJ).ToList();
                return DeltaDegreeAngle(values, anglesCount);
            }            
        }
        enum Stoch
        {
            Oscillator,
            Signal,
            PercentJ
        }
    }
}
