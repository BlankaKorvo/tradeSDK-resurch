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
        int dpoPeriod = 10;
        decimal lastDpoCondition = 0;
        int averageAngleCount = 3;
        double averageAngleConditionLong = 0;
        double averageAngleConditionFromLong = -30;

        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (
                DpoDegreeAverageAngle(dpo, averageAngleCount) > averageAngleConditionLong
                && DpoDegreeAverageAngle(dpo, 1) > averageAngleConditionLong
                )
            {
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionLong);
                Log.Information("Dpo Degree Average Angle = " + DpoDegreeAverageAngle(dpo, averageAngleCount) + " it should be more then: Average Angle Condition");
                Log.Information("Dpo Degree Last Average Angle = " + DpoDegreeAverageAngle(dpo, 1) + " it should be more then: Average Angle Condition");
                Log.Information("DPO = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionLong);
                Log.Information("Dpo Degree Average Angle = " + DpoDegreeAverageAngle(dpo, averageAngleCount) + " it should be more then: Average Angle Condition");
                Log.Information("Dpo Degree Last Average Angle = " + DpoDegreeAverageAngle(dpo, 1) + " it should be more then: Average Angle Condition");
                Log.Information("DPO = Long - false for: " + candleList.Figi);
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < lastDpoCondition
                || DpoDegreeAverageAngle(dpo, averageAngleCount) < averageAngleConditionFromLong)
            {
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionFromLong);
                Log.Information("Last DPO should be less then Last Dpo Condition");
                Log.Information("Dpo Degree Average Angle should be less then Average Angle Condition");
                Log.Information("DPO = FromLong - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionFromLong);
                Log.Information("Last DPO should be less then Last Dpo Condition");
                Log.Information("Dpo Degree Average Angle should be less then Average Angle Condition");
                Log.Information("DPO = FromLong - false for: " + candleList.Figi);
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

        internal bool LongSignalUltimate(CandleList candleList, decimal deltaPrice, double averageAngleConditionLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo >= lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) > averageAngleConditionLong
                && DpoDegreeAverageAngle(dpo, 1) >= DpoDegreeAverageAngle(dpo, 2))
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionLong);
                Log.Information("Last DPO > lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration > " + averageAngleConditionLong);
                Log.Information("Average Dpo Angle from 1 last iteration > from 2 last iteration");
                Log.Information("DPO = Long true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("DPO = Long false");
                return false;
            }
        }

        internal bool FromLongSignalUltimate(CandleList candleList, decimal deltaPrice, double averageAngleConditionFromLong)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) < averageAngleConditionFromLong)
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + lastDpoCondition);
                Log.Information("Average Angle Count" + averageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionFromLong);
                Log.Information("Last DPO < lastDpoCondition");
                Log.Information("Average Dpo Angle from " + averageAngleCount + " last iteration < " + averageAngleConditionFromLong);
                Log.Information("DPO = FromLong true for: " + candleList.Figi);
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
