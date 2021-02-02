using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffData;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class DpoSignal : IndicatorSignalsHelper
    {
        const int _dpoPeriod = 20;
        const decimal _lastDpoCondition = 0;
        const int _averageAngleCount = 3;
        const double _averageAngleConditionLong = 30;
        const double _averageAngleConditionFromLong = -30;

        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo >= lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) > averageAngleCondition
                && DpoDegreeAverageAngle(dpo, 1) >= DpoDegreeAverageAngle(dpo, 2))
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleCondition);
                Log.Information("Last DPO > lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration > " + averageAngleCondition);
                Log.Information("Average Dpo Angle from 1 last iteration > from 2 last iteration");
                return true;
            }
            else
            {
                Log.Information("DPO Long Signal not present");
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionFromLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) < averageAngleCondition)
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleCondition);
                Log.Information("Last DPO < lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration < " + averageAngleCondition);
                return true;
            }
            else
            {
                Log.Information("DPO From Long Signal not present");
                return false;
            }
        }

        double DpoDegreeAverageAngle(List<DpoResult> dpo, int anglesCount)
        {
            List<DpoResult> skipDpo = dpo.Skip(dpo.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipDpo)
            {
                values.Add(item.Dpo);
                Log.Information("DPO for Degree Average Angle: " + item.Date + " " + item.Dpo);
            }

            return DeltaDegreeAngle(values);
        }
    }
}
