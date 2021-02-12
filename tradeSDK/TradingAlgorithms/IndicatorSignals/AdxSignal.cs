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
    class AdxSignal : IndicatorSignalsHelper
    {
        int lookbackPeriod = 8;
        int averageAngleCount = 2;
        int fromLongAverageAngleCount = 2;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<AdxResult> adx = Serialization.AdxData(candleList, deltaPrice, lookbackPeriod);
            if (
                            adx.Last().Pdi > adx.Last().Mdi
                            &&
                            adx.Last().Adx > adx.Last().Mdi
                            &&
                            AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Adx) > 0
                            &&
                            AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Pdi) > 0
                            &&
                            AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Mdi) < 0
                            )
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date + " should be less then Adx Pdi и Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more Adx then Mdi");
                Log.Information("Adx angle " + averageAngleCount + "straights Adx = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Adx) + " should be more then 0");
                Log.Information("Adx angle " + averageAngleCount + "straights Pdi = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Pdi) + " should be more then 0");
                Log.Information("Adx angle " + averageAngleCount + "straights Mdi = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Mdi) + " should be less then 0");
                Log.Information("Adx = Long - true");
                return true;
            }
            else
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date + " should be less then Adx Pdi и Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx angle " + averageAngleCount + "straights Adx = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Adx) + " should be more then 0");
                Log.Information("Adx angle " + averageAngleCount + "straights Pdi = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Pdi) + " should be more then 0");
                Log.Information("Adx angle " + averageAngleCount + "straights Mdi = " + AdxDegreeAverageAngle(adx, averageAngleCount, Adx.Mdi) + " should be less then 0");
                Log.Information("Adx = Long - false");
                return false;
            }

        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<AdxResult> adx = Serialization.AdxData(candleList, deltaPrice, lookbackPeriod);
            if (
                    AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Adx) < 0
                    ||
                    (
                        AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Pdi) < 0
                        &&
                        AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Mdi) > 0
                    )
               )
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date );
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date );
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date );
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Adx = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Adx) + " should be less then 0");
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Pdi = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Pdi) + " should be less then 0");
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Mdi = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Mdi) + " should be more then 0");
                Log.Information("Adx = FromLong - true");
                return true;
            }
            else
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date + " should be less then Adx Pdi & Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Adx = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Adx) + " should be more then 0");
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Pdi = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Pdi) + " should be more then 0");
                Log.Information("Adx angle " + fromLongAverageAngleCount + "straights Mdi = " + AdxDegreeAverageAngle(adx, fromLongAverageAngleCount, Adx.Mdi) + " should be less then 0");
                Log.Information("Adx = FromLong - false");
                return false;
            }

        }
        private double AdxDegreeAverageAngle(List<AdxResult> AdxValue, int anglesCount, Adx adxLine)
        {
            List<AdxResult> skipAdx = AdxValue.Skip(AdxValue.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipAdx)
            {
                switch (adxLine)
                {
                    case Adx.Pdi:
                        values.Add(item.Pdi);
                        Log.Information("ADX +DI for Degree Average Angle: " + item.Date + " " + item.Pdi);
                        break;
                    case Adx.Mdi:
                        values.Add(item.Mdi);
                        Log.Information("ADX -DI for Degree Average Angle: " + item.Date + " " + item.Mdi);
                        break;
                    case Adx.Adx:
                        values.Add(item.Adx);
                        Log.Information("ADX for Degree Average Angle: " + item.Date + " " + item.Adx);
                        break;
                }
            }
            return DeltaDegreeAngle(values);
        }
        enum Adx
        {
            Pdi,
            Mdi,
            Adx
        }
    }
}
