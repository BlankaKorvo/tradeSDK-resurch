using Serilog;
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
    internal class IchimokuSignal : IndicatorSignalsHelper
    {
        internal bool LongSignal(CandleList candleList, decimal deltaPrice, decimal tenkansenPriceDeltaCount = 0.12M, int deltaAngleCountLong = 3, double ichimokuTenkanSenAngleLong = 20)
        {
            List<IchimokuResult> ichimoku = Serialization.IchimokuData(candleList, deltaPrice);
            decimal? tenkansenPriceDelta = 100 - (ichimoku.Last().TenkanSen * 100 / deltaPrice); //Насколько далеко убежала цена от Ichimoku TenkanSen

            if (ichimoku.Last().TenkanSen > ichimoku.Last().KijunSen
                            && deltaPrice > ichimoku.Last().SenkouSpanA
                            && deltaPrice > ichimoku.Last().SenkouSpanB
                            && deltaPrice > ichimoku.Last().TenkanSen
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanA
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanB
                            && ichmokuTenkansenDegreeAverageAngle(ichimoku, deltaAngleCountLong) > ichimokuTenkanSenAngleLong
                            && ichmokuTenkansenDegreeAverageAngle(ichimoku, 1) > ichmokuTenkansenDegreeAverageAngle(ichimoku, 2)
                            && tenkansenPriceDelta < tenkansenPriceDeltaCount)
            {
                Log.Information("Tenkansen Price Delta Count = " + tenkansenPriceDeltaCount);
                Log.Information("Delta Angle Count Long = " + deltaAngleCountLong);
                Log.Information("Ichimoku TenkanSen Angle Long = " + ichimokuTenkanSenAngleLong);
                Log.Information("Ichimoku Long Signal = Buy");
                return true;
            }
            else
            {
                Log.Information("Ichimoku Long Signal = Not Buy");
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int deltaAngleCountLong = 1, double ichimokuTenkanSenAngleLong = 0)
        {
            List<IchimokuResult> ichimoku = Serialization.IchimokuData(candleList, deltaPrice);
            if (/*ichimoku.Last().TenkanSen < ichimoku.Last().KijunSen*/ //под вопросом на минунтных свечах. Достаточно отрицательный угол на последнем отрезке. Возможно, потребуется, если буду нормализовать свечи не по клозу, а по средней цене за свечу.
                            /*||*/ deltaPrice < ichimoku.Last().SenkouSpanA
                            || deltaPrice < ichimoku.Last().SenkouSpanB
                            || deltaPrice < ichimoku.Last().TenkanSen
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanA
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanB
                            || ichmokuTenkansenDegreeAverageAngle(ichimoku, deltaAngleCountLong) < ichimokuTenkanSenAngleLong)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        double ichmokuTenkansenDegreeAverageAngle(List<IchimokuResult> ichimoku, int anglesCount)
        {
            List<IchimokuResult> skipIchimoku = ichimoku.Skip(ichimoku.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipIchimoku)
            {
                values.Add(item.TenkanSen);
                Log.Information("Tenkansen: " + item.Date + " " + item.TenkanSen);
            }
            return DeltaDegreeAngle(values);
        }
    }
}
