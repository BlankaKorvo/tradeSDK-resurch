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
    public partial class Signal : IndicatorSignalsHelper, ISignal
    {
        int adxLookbackPeriod = 14;
        int adxAverageAngleCount = 1;
        int adxFromLongAverageAngleCount = 2;
        double pdiAngleLong = 20;
        double mdiAngleLong = -20;
        double adxAngleLong = 0;
        int expecCountAdxCandles = 2;

        public bool AdxLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start AdxSignal LongSignal method with figi:" + candleList.Figi);
            List<AdxResult> adx = Mapper.AdxData(candleList, deltaPrice, adxLookbackPeriod);
            if (adx == null)
            {
                Log.Information("Adx = null");
                Log.Information("Adx = Long - false for: " + candleList.Figi);
                Log.Information("Stop AdxSignal LongSignal method with figi:" + candleList.Figi);
                return false;
            }

            int count = adx.Count();
            int countAdxCandles = 0;
            for (int i = 1; i < count; i++)
            {
                countAdxCandles += 1;
                List<AdxResult> takeAdx = adx.Take(count - i).ToList();
                if (takeAdx.Last().Pdi < takeAdx.Last().Mdi)
                {
                      break;
                }
            }

            if (adx==null) { return false; }

            decimal? pdiLast = adx.Last().Pdi;
            decimal? mdiLast = adx.Last().Mdi;
            List<AdxResult> adxMinusOne = adx.Take(count - 2).ToList();
            double adxDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Adx);
            double pdiDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Pdi);
            double mdiDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxAverageAngleCount, Adx.Mdi);

            double adxDegreeAverageAnglePreLast = AdxDegreeAverageAngle(adxMinusOne, adxAverageAngleCount, Adx.Adx);
            double pdiDegreeAverageAnglePreLast = AdxDegreeAverageAngle(adxMinusOne, adxAverageAngleCount, Adx.Pdi);
            double mdiDegreeAverageAnglePreLast = AdxDegreeAverageAngle(adxMinusOne, adxAverageAngleCount, Adx.Mdi);

            Log.Information("Adx Pdi = " + pdiLast + " " + adx.Last().Date + " should be more then " + mdiLast);
            Log.Information("Adx Mdi = " + mdiLast + " " + adx.Last().Date + " should be less then " + pdiLast);
            //Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date + " should be more Adx then Mdi");

            Log.Information("Adx angle " + adxAverageAngleCount + " straights Adx = " + adxDegreeAverageAngleLast + " should be more then " + adxAngleLong);
            Log.Information("Adx angle " + adxAverageAngleCount + " straights Pdi = " + pdiDegreeAverageAngleLast + " should be more then " + pdiAngleLong);
            Log.Information("Adx angle " + adxAverageAngleCount + " straights Mdi = " + mdiDegreeAverageAngleLast + " should be less then " + mdiAngleLong);

            Log.Information("Adx angle " + adxAverageAngleCount + " straights Adx = " + adxDegreeAverageAngleLast + " should be more then adxDegreeAverageAnglePreLast: " + adxDegreeAverageAnglePreLast);
            Log.Information("Adx angle " + adxAverageAngleCount + " straights Pdi = " + pdiDegreeAverageAngleLast + " should be more then pdiDegreeAverageAnglePreLast: " + pdiDegreeAverageAnglePreLast);
            Log.Information("Adx angle " + adxAverageAngleCount + " straights Mdi = " + mdiDegreeAverageAngleLast + " should be less then mdiDegreeAverageAnglePreLast: " + mdiDegreeAverageAnglePreLast);

            Log.Information("countAdxCandles after pDi < mDi = " + countAdxCandles + " should be less then expecCountAdxCandles " + expecCountAdxCandles);


            if (
                            pdiLast > mdiLast
                            //&&
                            //adx.Last().Adx > mdiLast
                            //&&
                            //adx.Last().Adx > 25
                            &&
                            //adx.Last().Adx < adx.Last().Pdi
                            //&&
                            //adx.Last().Adx < adx.Last().Mdi  //очень сспорное решение
                            //&&
                            adxDegreeAverageAngleLast > adxAngleLong
                            &&
                            pdiDegreeAverageAngleLast > pdiAngleLong
                            &&
                            mdiDegreeAverageAngleLast < mdiAngleLong
                            &&
                            adxDegreeAverageAngleLast > adxDegreeAverageAnglePreLast
                            &&
                            pdiDegreeAverageAngleLast > pdiDegreeAverageAnglePreLast
                            &&
                            mdiDegreeAverageAngleLast < mdiDegreeAverageAnglePreLast
                            //&&
                            //countAdxCandles <= expecCountAdxCandles
                            )
            {

                Log.Information("Adx = Long - true for: " + candleList.Figi);
                Log.Information("Stop AdxSignal LongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else
            {

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


            decimal? pdiLast = adx.Last().Pdi;
            decimal? mdiLast = adx.Last().Mdi;
            double adxDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Adx);
            double pdiDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Pdi);
            double mdiDegreeAverageAngleLast = AdxDegreeAverageAngle(adx, adxFromLongAverageAngleCount, Adx.Mdi);

            Log.Information("Adx Pdi = " + adx.Last().Pdi + " " + adx.Last().Date);
            Log.Information("Adx Mdi = " + adx.Last().Mdi + " " + adx.Last().Date);
            Log.Information("Adx Adx = " + adx.Last().Adx + " " + adx.Last().Date);

            Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Adx = " + adxDegreeAverageAngleLast + " should be less then " + 0);
            Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Pdi = " + pdiDegreeAverageAngleLast + " should be less then " + 0);
            Log.Information("Adx angle " + adxFromLongAverageAngleCount + " straights Mdi = " + mdiDegreeAverageAngleLast + " should be less then " + 0);


            if (
                    adxDegreeAverageAngleLast <= 0
                    ||
                    (
                        pdiDegreeAverageAngleLast < 0
                        &&
                        mdiDegreeAverageAngleLast > 0
                    )
               )
            {


                Log.Information("Adx = FromLong - true for: " + candleList.Figi);
                Log.Information("Stop AdxSignal FromLongSignal method with figi:" + candleList.Figi);
                return true;
            }
            else
            {

                Log.Information("Adx = FromLong - false for: " + candleList.Figi);
                Log.Information("Stop AdxSignal FromLongSignal method with figi:" + candleList.Figi);
                return false;
            }

        }
        public double AdxDegreeAverageAngle(List<AdxResult> AdxValue, int anglesCount, Adx adxLine)
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
        public enum Adx
        {
            Pdi,
            Mdi,
            Adx
        }
    }
}
