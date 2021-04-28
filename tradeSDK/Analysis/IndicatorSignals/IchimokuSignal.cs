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
        const decimal ichimokuTenkansenPriceDeltaCount = 0.12M;
        const int ichimokuDeltaAngleCountLong = 1;
        const double ichimokuTenkanSenAngleLong = 0;

        const int ichimokuDeltaAngleCountFromLong = 1;
        const double ichimokuTenkanSenAngleFromLong = 0;

        internal bool IchimokuLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            List<IchimokuResult> ichimoku = Mapper.IchimokuData(candleList, deltaPrice);
            decimal? tenkansenPriceDelta = 100 - (ichimoku.Last().TenkanSen * 100 / deltaPrice); //Насколько далеко убежала цена от Ichimoku TenkanSen

            if (ichimoku.Last().TenkanSen > ichimoku.Last().KijunSen
                            && deltaPrice > ichimoku.Last().SenkouSpanA
                            && deltaPrice > ichimoku.Last().SenkouSpanB
                            && deltaPrice > ichimoku.Last().TenkanSen
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanA
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanB
                            && IchimokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong) > ichimokuTenkanSenAngleLong
                            //&& ichmokuTenkansenDegreeAverageAngle(ichimoku, 1) > ichmokuTenkansenDegreeAverageAngle(ichimoku, 2)
                            //&& tenkansenPriceDelta < ichimokuTenkansenPriceDeltaCount
                            )
            {
                Log.Information("Tenkansen Price Delta Count = " + ichimokuTenkansenPriceDeltaCount);
                Log.Information("Delta Angle Count Long = " + ichimokuDeltaAngleCountLong);
                Log.Information("Ichimoku TenkanSen Angle = " + ichimokuTenkanSenAngleLong);
                Log.Information("Ichimoku TenkanSen Degree Average Angle ( " + ichimokuDeltaAngleCountLong + " ) = " + IchimokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong));
                Log.Information("Ichimoku = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Tenkansen Price Delta Count = " + ichimokuTenkansenPriceDeltaCount);
                Log.Information("Delta Angle Count = " + ichimokuDeltaAngleCountLong);
                Log.Information("Ichimoku TenkanSen Angle = " + ichimokuTenkanSenAngleLong);
                Log.Information("Ichimoku = Long - false for: " + candleList.Figi);
                return false;
            }
        }

        internal bool IchimokuFromLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            List<IchimokuResult> ichimoku = Mapper.IchimokuData(candleList, deltaPrice);
            if (/*ichimoku.Last().TenkanSen < ichimoku.Last().KijunSen*/ //под вопросом на минунтных свечах. Достаточно отрицательный угол на последнем отрезке. Возможно, потребуется, если буду нормализовать свечи не по клозу, а по средней цене за свечу.
                            /*||*/ deltaPrice < ichimoku.Last().SenkouSpanA
                            || deltaPrice < ichimoku.Last().SenkouSpanB
                            || deltaPrice < ichimoku.Last().TenkanSen
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanA
                            || ichimoku.Last().TenkanSen < ichimoku.Last().SenkouSpanB
                            || IchimokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountFromLong) < ichimokuTenkanSenAngleFromLong)
            {
                Log.Information("Ichimoku = FromLong - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Ichimoku = FromLong - false for: " + candleList.Figi);
                return false;
            }
        }

        double IchimokuTenkansenDegreeAverageAngle(List<IchimokuResult> ichimoku, int anglesCount)
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
