using MarketDataModules;
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
    public partial class Signal : IndicatorSignalsHelper
    {
        int dpoPeriod = 10;
        decimal dpoLastDpoCondition = 0;
        int dpoAverageAngleCount = 3;
        double dpoAverageAngleConditionLong = 0;
        double dpoAverageAngleConditionFromLong = -30;

        internal bool DpoLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Dpo LongSignal. Figi: " + candleList.Figi);
            List<DpoResult> dpo = Mapper.DpoData(candleList, deltaPrice, dpoPeriod);
            double dpoDegreeAverageAngle = DpoDegreeAverageAngle(dpo, dpoAverageAngleCount);
            double dpoDegreeAverageAngle1 = DpoDegreeAverageAngle(dpo, 1);
            if (
                dpoDegreeAverageAngle > dpoAverageAngleConditionLong
                && dpoDegreeAverageAngle1 > dpoAverageAngleConditionLong
                )
            {
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + dpoAverageAngleConditionLong);
                Log.Information("Dpo Degree Average Angle = " + dpoDegreeAverageAngle + " it should be more then: Average Angle Condition");
                Log.Information("Dpo Degree Last Average Angle = " + dpoDegreeAverageAngle1 + " it should be more then: Average Angle Condition");
                Log.Information("Dpo LongSignal = Long - true. Figi: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + dpoAverageAngleConditionLong);
                Log.Information("Dpo Degree Average Angle = " + dpoDegreeAverageAngle + " it should be more then: Average Angle Condition");
                Log.Information("Dpo Degree Last Average Angle = " + dpoDegreeAverageAngle1 + " it should be more then: Average Angle Condition");
                Log.Information("Dpo LongSignal = Long - false. Figi: " + candleList.Figi);
                return false;
            }
        }

        internal bool DpoFromLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start Dpo FromLongSignal. Figi: " + candleList.Figi);
            List<DpoResult> dpo = Mapper.DpoData(candleList, deltaPrice, dpoPeriod);
            double dpoDegreeAverageAngle = DpoDegreeAverageAngle(dpo, dpoAverageAngleCount);
            if (dpo.Last().Dpo < dpoLastDpoCondition
                || dpoDegreeAverageAngle < dpoAverageAngleConditionFromLong)
            {
                Log.Information("Last Dpo Condition = " + dpoLastDpoCondition);
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + dpoAverageAngleConditionFromLong);
                Log.Information("Last DPO should be less then Last Dpo Condition");
                Log.Information("Dpo Degree Average Angle should be less then Average Angle Condition");
                Log.Information("Dpo FromLongSignal = Long - true. Figi: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Last Dpo Condition = " + dpoLastDpoCondition);
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + dpoAverageAngleConditionFromLong);
                Log.Information("Last DPO should be less then Last Dpo Condition");
                Log.Information("Dpo Degree Average Angle should be less then Average Angle Condition");
                Log.Information("Dpo FromLongSignal = Long - false. Figi: " + candleList.Figi);
                return false;
            }
        }

        double DpoDegreeAverageAngle(List<DpoResult> dpo, int anglesCount)
        {
            Log.Information("Start DpoDegreeAverageAngle");
            List<DpoResult> skipDpo = dpo.Skip(dpo.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipDpo)
            {
                values.Add(item.Dpo);
                Log.Information("DPO for Degree Average Angle: " + item.Date + " " + item.Dpo);
            }
            Log.Information("Stop DpoDegreeAverageAngle");
            return DeltaDegreeAngle(values);
        }

        internal bool DpoLongSignalUltimate(CandlesList candleList, decimal deltaPrice, double averageAngleConditionLong)
        {
            List<DpoResult> dpo = Mapper.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo >= dpoLastDpoCondition
                && DpoDegreeAverageAngle(dpo, dpoAverageAngleCount) > averageAngleConditionLong
                && DpoDegreeAverageAngle(dpo, 1) >= DpoDegreeAverageAngle(dpo, 2))
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + dpoLastDpoCondition);
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionLong);
                Log.Information("Last DPO > lastDpoCondition");
                Log.Information("Average Dpo Angle from " + dpoAverageAngleCount + " last iteration > " + averageAngleConditionLong);
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

        internal bool DpoFromLongSignalUltimate(CandlesList candleList, decimal deltaPrice, double averageAngleConditionFromLong)
        {
            List<DpoResult> dpo = Mapper.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < dpoLastDpoCondition
                && DpoDegreeAverageAngle(dpo, dpoAverageAngleCount) < averageAngleConditionFromLong)
            {
                Log.Information("Dpo Period = " + dpoPeriod);
                Log.Information("Last Dpo Condition = " + dpoLastDpoCondition);
                Log.Information("Average Angle Count" + dpoAverageAngleCount);
                Log.Information("Average Angle Condition" + averageAngleConditionFromLong);
                Log.Information("Last DPO < lastDpoCondition");
                Log.Information("Average Dpo Angle from " + dpoAverageAngleCount + " last iteration < " + averageAngleConditionFromLong);
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
