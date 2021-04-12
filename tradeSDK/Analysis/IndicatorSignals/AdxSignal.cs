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
    partial class Signal : IndicatorSignalsHelper, ISignal
    {
        int adxLookbackPeriod = 8;
        int adxAverageAngleCount = 2;
        int adxFromLongAverageAngleCount = 2;
        double pdiAngleLong = 50;
        int expecCountAdxCandles = 2;

        public bool AdxLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start AdxSignal LongSignal method with figi:" + candleList.Figi);
            List<AdxResult> adx = Mapper.AdxData(candleList, deltaPrice, adxLookbackPeriod);

            int count = adx.Count();
            int countAdxCandles = 0;
            for (int i = 1; i < count; i++)
            {
                List<AdxResult> takeAdx = adx.Take(count - i).ToList();
                if (AdxDegreeAverageAngle(takeAdx, adxAverageAngleCount, Adx.Adx) < 0)
                {
                    countAdxCandles = i - 1;
                    break;
                }
            }

            if (adx==null) { return false; }

            decimal? pdiLast = adx.Last().Pdi;
            decimal? mdiLast = adx.Last().Mdi;
            double adxDegreeAverageAngle = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Adx);
            double pdiDegreeAverageAngle = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Pdi);


            double mdiDegreeAverageAngle = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Mdi);
            if (
                            pdiLast > mdiLast
                            &&
                            adx.Last().Adx > mdiLast
                            &&
                            adxDegreeAverageAngle > 0
                            &&
                            pdiDegreeAverageAngle > pdiAngleLong
                            &&
                            mdiDegreeAverageAngle < 0
                            &&
                            countAdxCandles <= expecCountAdxCandles
                            )
            {
                Log.Information("Adx Pdi = " + pdiLast + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + mdiLast + " " + adx.Last().Date + " should be less then Adx Pdi и Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more Adx then Mdi");
                Log.Information("countAdxCandles = " + countAdxCandles + " should be less then expecCountAdxCandles "+ expecCountAdxCandles);
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Adx = " + adxDegreeAverageAngle + " should be more then 0");
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Pdi = " + pdiDegreeAverageAngle + " should be more then " + pdiAngleLong);
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Mdi = " + mdiDegreeAverageAngle + " should be less then 0");
                Log.Information("Adx = Long - true for: " + candleList.Figi);
                Log.Information("Stop AdxSignal LongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Adx Pdi = " + pdiLast + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + mdiLast + " " + adx.Last().Date + " should be less then Adx Pdi и Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("countAdxCandles = " + countAdxCandles + " should be less then expecCountAdxCandles " + expecCountAdxCandles);
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Adx = " + adxDegreeAverageAngle + " should be more then 0");
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Pdi = " + pdiDegreeAverageAngle + " should be more then " + pdiAngleLong);
                Log.Information("Adx angle " + adxAverageAngleCount + " straights Mdi = " + mdiDegreeAverageAngle + " should be less then 0");
                Log.Information("Adx = Long - false for: " + candleList.Figi);
                Log.Information("Stop AdxSignal LongSignal method with figi:" + candleList.Figi);
                return false;
            }

        }

        public bool AdxFromLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start AdxSignal FromLongSignal method with figi:" + candleList.Figi);
            List<AdxResult> adx = Mapper.AdxData(candleList, deltaPrice, adxLookbackPeriod);
            if (adx == null) { return false; }
            if (
                    AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Adx) < 0
                    ||
                    (
                        AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Pdi) < 0
                        &&
                        AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Mdi) > 0
                    )
               )
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date );
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date );
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date );
                //Log.Information("countAdxCandles = " + countAdxCandles + " should be less then expecCountAdxCandles " + expecCountAdxCandles);
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Adx = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Adx) + " should be less then 0");
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Pdi = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Pdi) + " should be less then" + pdiAngleLong);
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Mdi = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Mdi) + " should be more then 0");
                Log.Information("Adx = FromLong - true for: " + candleList.Figi);
                Log.Information("Stop AdxSignal FromLongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date + " should be more then Adx Mdi");
                Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date + " should be less then Adx Pdi & Adx");
                Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more then Adx Mdi");
                //Log.Information("countAdxCandles = " + countAdxCandles + " should be less then expecCountAdxCandles " + expecCountAdxCandles);
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Adx = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Adx) + " should be more then 0");
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Pdi = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Pdi) + " should be more then" + pdiAngleLong);
                Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Mdi = " + AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Mdi) + " should be less then 0");
                Log.Information("Adx = FromLong - false for: " + candleList.Figi);
                Log.Information("Stop AdxSignal FromLongSignal method with figi:" + candleList.Figi);
                return false;
            }

        }
        double AdxDegreeAverageAngle(List<AdxResult> AdxValue, int anglesCount, Adx adxLine)
        {
            Log.Information("Start AdxSignal AdxDegreeAverageAngle method");
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
            Log.Information("Stop AdxSignal AdxDegreeAverageAngle method");
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
