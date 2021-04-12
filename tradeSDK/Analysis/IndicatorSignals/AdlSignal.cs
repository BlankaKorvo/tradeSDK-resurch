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
        int adlLookbackPeriodSma = 5;
        int adlAnglesCount = 10;

        internal bool AdlLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Adl LongSignal. Figi: " + candleList.Figi);
            List<AdlResult> adl = Mapper.AdlData(candleList, deltaPrice, adlLookbackPeriodSma);
            double adlDegreeAverageAngle = AdlDegreeAverageAngle(adl, adlAnglesCount, Adl.Adl);
            if (
                //adl.Last().Adl > 0
                //&&
                adlDegreeAverageAngle > 0
               )
            {
                Log.Information("Adl = " + adl.Last().Adl);
                Log.Information("AdlDegreeAverageAngle = " + adlDegreeAverageAngle + " in "+ adlAnglesCount + " count. It must be > 0 for long");
                return true;
            }
            else
            {
                Log.Information("Adl = " + adl.Last().Adl);
                Log.Information("AdlDegreeAverageAngle = " + adlDegreeAverageAngle + " in " + adlAnglesCount + " count. It must be > 0 for long");
                return false;
            }
        }
        double AdlDegreeAverageAngle(List<AdlResult> AdlValue, int anglesCount, Adl line)
        {
            if (line == Adl.Adl)
            {
                List<decimal?> values = AdlValue.Select(na => (decimal?)na.Adl).ToList();
                return DeltaDegreeAngle(values, anglesCount);
            }
            else
            {
                List<decimal?> values = AdlValue.Select(na => (decimal?)na.AdlSma).ToList();
                return DeltaDegreeAngle(values, anglesCount);
            }
        }
        enum Adl
        {
            Adl,
            AdlSma,
        }
    }
}
