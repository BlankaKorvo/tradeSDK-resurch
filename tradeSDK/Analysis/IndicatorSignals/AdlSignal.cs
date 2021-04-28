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
        int adlLookbackPeriodSma = 5;
        int adlAnglesCountLongMax = 10;
        int adlAnglesCountFromLong = 1;
        int adlAnglesCountLongMin = 3;
        double adlAverageAngleConditionLong = 0;

        internal bool AdlLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Adl LongSignal. Figi: " + candleList.Figi);
            List<AdlResult> adl = Mapper.AdlData(candleList, deltaPrice, adlLookbackPeriodSma);
            double adlDegreeAverageAngleMax = AdlDegreeAverageAngle(adl, adlAnglesCountLongMax, Adl.Adl);
            double adlDegreeAverageAngleMin = AdlDegreeAverageAngle(adl, adlAnglesCountLongMin, Adl.Adl);
            double adlDegreeAverageAngle1 = AdlDegreeAverageAngle(adl, 1, Adl.Adl);
            if (
                //adl.Last().Adl > 0
                //&&
                adlDegreeAverageAngleMax > adlAverageAngleConditionLong
                &&
                adlDegreeAverageAngleMin > adlAverageAngleConditionLong
                &&
                adlDegreeAverageAngle1 > adlAverageAngleConditionLong
               )
            {
                Log.Information("Adl = " + adl.Last().Adl);
                Log.Information("AdlDegreeAverageAngle = " + adlDegreeAverageAngleMax + " in "+ adlAnglesCountLongMax + " count. It must be > 0 for long");
                Log.Information("Adl = Long - true for: " + candleList.Figi);
                Log.Information("Stop AdlSignal LongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Adl = " + adl.Last().Adl);
                Log.Information("AdlDegreeAverageAngle = " + adlDegreeAverageAngleMax + " in " + adlAnglesCountLongMax + " count. It must be > 0 for long");
                Log.Information("Adl = Long - false for: " + candleList.Figi);
                Log.Information("Stop AdlSignal LongSignal method with figi:" + candleList.Figi);
                return false;
            }
        }

        internal bool AdlFromLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Adl FromLongSignal. Figi: " + candleList.Figi);
            List<AdlResult> adl = Mapper.AdlData(candleList, deltaPrice, adlLookbackPeriodSma);
            double adlDegreeAverageAngle = AdlDegreeAverageAngle(adl, adlAnglesCountFromLong, Adl.Adl);
            Log.Information("Adl = " + adl.Last().Adl);
            Log.Information("AdlDegreeAverageAngle = " + adlDegreeAverageAngle + " in " + adlAnglesCountLongMax + " count. It must be < 0 for fromLong");
            if (adlDegreeAverageAngle < 0)
            {
                Log.Information("Adl = FromLong - true for: " + candleList.Figi);
                Log.Information("Stop AdlSignal FromLongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else 
            {
                Log.Information("Adl = FromLong - false for: " + candleList.Figi);
                Log.Information("Stop AdlSignal FromLongSignal method with figi:" + candleList.Figi);
                return false;
            }
        }


        public double AdlDegreeAverageAngle(List<AdlResult> AdlValue, int anglesCount, Adl line)
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
        public enum Adl
        {
            Adl,
            AdlSma,
        }
    }
}
