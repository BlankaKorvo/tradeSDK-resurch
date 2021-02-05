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
    internal class IchimokuSignal : IndicatorSignalsHelper
    {
        const decimal _tenkansenPriceDeltaCount = 0.12M;
        const int _deltaAngleCountLong = 1;
        const double _ichimokuTenkanSenAngleLong = 0;

        const int _deltaAngleCountFromLong = 1;
        const double _ichimokuTenkanSenAngleFromLong = 0;

        internal bool LongSignal(CandleList candleList, decimal deltaPrice, decimal tenkansenPriceDeltaCount = _tenkansenPriceDeltaCount, int deltaAngleCountLong = _deltaAngleCountLong, double ichimokuTenkanSenAngleLong = _ichimokuTenkanSenAngleLong)
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
                Log.Information("Ichimoku TenkanSen Angle = " + ichimokuTenkanSenAngleLong);
                Log.Information("Ichimoku TenkanSen Degree Average Angle ( " + deltaAngleCountLong + " ) = " + ichmokuTenkansenDegreeAverageAngle(ichimoku, deltaAngleCountLong));
                Log.Information("Ichimoku = Long true");
                return true;
            }
            else
            {
                Log.Information("Tenkansen Price Delta Count = " + tenkansenPriceDeltaCount);
                Log.Information("Delta Angle Count = " + deltaAngleCountLong);
                Log.Information("Ichimoku TenkanSen Angle = " + ichimokuTenkanSenAngleLong);
                Log.Information("Ichimoku = Long false");
                return false;
            }
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int deltaAngleCountFromLong = _deltaAngleCountFromLong, double ichimokuTenkanSenAngleFromLong = _ichimokuTenkanSenAngleFromLong)
        {
            List<IchimokuResult> ichimoku = Serialization.IchimokuData(candleList, deltaPrice);
            if (/*ichimoku.Last().TenkanSen < ichimoku.Last().KijunSen*/ //под вопросом на минунтных свечах. Достаточно отрицательный угол на последнем отрезке. Возможно, потребуется, если буду нормализовать свечи не по клозу, а по средней цене за свечу.
                            /*||*/ deltaPrice < ichimoku.Last().SenkouSpanA
                            || deltaPrice < ichimoku.Last().SenkouSpanB
                            || deltaPrice < ichimoku.Last().TenkanSen
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanA
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanB
                            || ichmokuTenkansenDegreeAverageAngle(ichimoku, deltaAngleCountFromLong) < ichimokuTenkanSenAngleFromLong)
            {
                Log.Information("Ichimoku = FromLong true");
                return true;
            }
            else
            {
                Log.Information("Ichimoku = FromLong false");
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
