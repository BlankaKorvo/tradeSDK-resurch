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
        int adlAnglesCount = 2;

        internal bool AdlLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start OBV LongSignal. Figi: " + candleList.Figi);
            List<AdlResult> adl = Mapper.AdlData(candleList, deltaPrice, adlLookbackPeriodSma);

            if (
                adl.Last().Adl > 0
                &&
                AdlDegreeAverageAngle(adl, adlAnglesCount, Adl.Adl) > 0
               )
            {       
                return true;
            }
            else
            {               
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
