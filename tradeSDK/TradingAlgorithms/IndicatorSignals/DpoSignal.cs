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
        const int _dpoPeriod = 10;
        const decimal _lastDpoCondition = 0;
        const int _averageAngleCount = 3;
        const double _averageAngleConditionLong = 0;
        const double _averageAngleConditionFromLong = -30;

        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (
                //dpo.Last().Dpo >= lastDpoCondition && 
                DpoDegreeAverageAngle(dpo, averageAngleCount) > averageAngleCondition
                && DpoDegreeAverageAngle(dpo, 1) > averageAngleCondition
                //&& DpoDegreeAverageAngle(dpo, 1) >= DpoDegreeAverageAngle(dpo, 2)
                )
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleCondition);
                Log.Information("Last DPO > lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration > " + averageAngleCondition);
                Log.Information("Average Dpo Angle from 1 last iteration > from 2 last iteration");
                Log.Information("DPO = Long true");
                return true;
            }
            else
            {
                Log.Information("DPO = Long false");
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionFromLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < lastDpoCondition
                || DpoDegreeAverageAngle(dpo, averageAngleCount) < averageAngleCondition)
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleCondition);
                Log.Information("Last DPO < lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration < " + averageAngleCondition);
                Log.Information("DPO = FromLong true");
                return true;
            }
            else
            {
                Log.Information("DPO = FromLong false");
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

        internal bool LongSignalUltimate(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionLong)
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
                Log.Information("DPO = Long true");
                return true;
            }
            else
            {
                Log.Information("DPO = Long false");
                return false;
            }
        }

        internal bool FromLongSignalUltimate(CandleList candleList, decimal deltaPrice, int dpoPeriod = _dpoPeriod, decimal lastDpoCondition = _lastDpoCondition, int averageAngleCount = _averageAngleCount, double averageAngleCondition = _averageAngleConditionFromLong)
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
                Log.Information("DPO = FromLong true");
                return true;
            }
            else
            {
                Log.Information("DPO = FromLong false");
                return false;
            }
        }
    }    
}
