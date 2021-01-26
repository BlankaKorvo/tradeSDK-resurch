﻿using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class DpoSignal : IndicatorSignalsHelper
    {
        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod, decimal lastDpoCondition = 0, int averageAngleCount = 3, double averageAngleCondition = 30)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo >= lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) > averageAngleCondition)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int dpoPeriod, decimal lastDpoCondition = 0, int averageAngleCount = 4, double averageAngleCondition = -30)
        {
            List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
            if (dpo.Last().Dpo < lastDpoCondition
                && DpoDegreeAverageAngle(dpo, averageAngleCount) < averageAngleCondition)
            {
                return true;
            }
            else
            {
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
                Log.Information("DPO: " + item.Date + " " + item.Dpo);
            }

            return DeltaDegreeAngle(values);
        }
    }
}
