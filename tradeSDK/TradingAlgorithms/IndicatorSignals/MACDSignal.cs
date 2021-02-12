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
    internal class MacdSignal : IndicatorSignalsHelper
    {
        int fastPeriod = 6;
        int slowPeriod = 13;
        int signalPeriod = 9;
        int averageAngleCount = 2;
        double averageAngleCondition = 0;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<MacdResult> macd = Serialization.MacdData(candleList, deltaPrice, fastPeriod, slowPeriod, signalPeriod);

            if (
                macd.Last().Macd > macd.Last().Signal
                && macd.Last().Histogram > 0
                && MacdDegreeAverageAngle(macd, averageAngleCount) >= averageAngleCondition
                && MacdDegreeAverageAngle(macd, 1) > averageAngleCondition
                && macd.Last().Histogram > macd[macd.Count - 2].Histogram
                )
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average of " + averageAngleCount+ " Angle is degree:  " + MacdDegreeAverageAngle(macd, averageAngleCount) + " >= " + averageAngleCondition);
                Log.Information("Macd Average Angle is degree:  " + MacdDegreeAverageAngle(macd, 1) + " >= " + averageAngleCondition);
                //Log.Information("Macd Histogram Average Angle is degree:  " + MacdHistogramDegreeAverageAngle(macd, 1) + " >= " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Last Histogram MACD is biger, then prelast: " + macd.Last().Histogram  + " > " + macd[macd.Count - 2].Histogram);
                Log.Information("Macd = Long - true");
                return true;
            }
            else
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is not degree:  " + MacdDegreeAverageAngle(macd, 1) + " >= " + averageAngleCondition);
                Log.Information("Macd Average of " + averageAngleCount + " Angle is not degree:  " + MacdDegreeAverageAngle(macd, averageAngleCount) + " >= " + averageAngleCondition);
                Log.Information("Last Histogram MACD is lowest, then prelast: " + macd.Last().Histogram + " < " + macd[macd.Count - 2].Histogram);
                Log.Information("Macd = Long - false");
                return false;
            }            
        }

        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<MacdResult> macd = Serialization.MacdData(candleList, deltaPrice);

            if (
                macd.Last().Macd < macd.Last().Signal
                || macd.Last().Histogram < 0
                || MacdDegreeAverageAngle(macd, averageAngleCount) < averageAngleCondition
                //|| macd.Last().Histogram < macd[macd.Count - 2].Histogram
                //|| MacdHistogramDegreeAverageAngle(macd, 1) < MacdHistogramDegreeAverageAngle(macd, 2)
                )
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is not degree:  " + MacdDegreeAverageAngle(macd, 1) + " < 0" );
                Log.Information("Macd Histogram Average Angle is not degree:  " + MacdHistogramDegreeAverageAngle(macd, 1) + " < " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Macd = FromLong - true");
                return true;
            }
            else
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is degree:  " + MacdDegreeAverageAngle(macd, 1) + " >= 0");
                Log.Information("Macd Histogram Average Angle is degree:  " + MacdHistogramDegreeAverageAngle(macd, 1) + " < " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Macd = FromLong - false");
                return false;
            }
        }

        double MacdDegreeAverageAngle(List<MacdResult> macd, int anglesCount)
        {
            List<MacdResult> skipMacd = macd.Skip(macd.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipMacd)
            {
                values.Add(item.Macd);
                Log.Information("MACD for Degree Average Angle: " + item.Date + " " + item.Macd);
            }
            return DeltaDegreeAngle(values);
        }
        double MacdHistogramDegreeAverageAngle(List<MacdResult> macd, int anglesCount)
        {
            List<MacdResult> skipMacd = macd.Skip(macd.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipMacd)
            {
                values.Add(item.Histogram);
                Log.Information("MACD Histogram for Degree Average Angle: " + item.Date + " " + item.Histogram);
            }
            return DeltaDegreeAngle(values);
        }
    }
}
