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
        int macdFastPeriod = 6;
        int macdSlowPeriod = 13;
        int macdSignalPeriod = 9;
        int macdAverageAngleCount = 2;
        double macdAverageAngleCondition = 0;
        internal bool MacdLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start MACD LongSignal. Figi: " + candleList.Figi);
            List<MacdResult> macd = Mapper.MacdData(candleList, deltaPrice, macdFastPeriod, macdSlowPeriod, macdSignalPeriod);

            double macdDegreeAverageAngle = MacdDegreeAverageAngle(macd, macdAverageAngleCount);
            double macdDegreeAverageAngle1 = MacdDegreeAverageAngle(macd, 1);
            if (
                macd.Last().Macd > macd.Last().Signal
                && macd.Last().Histogram > 0
                && macdDegreeAverageAngle >= macdAverageAngleCondition
                && macdDegreeAverageAngle1 > macdAverageAngleCondition
                && macd.Last().Histogram > macd[macd.Count - 2].Histogram
                )
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average of " + macdAverageAngleCount + " Angle is degree:  " + macdDegreeAverageAngle + " >= " + macdAverageAngleCondition);
                Log.Information("Macd Average Angle is degree:  " + macdDegreeAverageAngle1 + " >= " + macdAverageAngleCondition);
                //Log.Information("Macd Histogram Average Angle is degree:  " + MacdHistogramDegreeAverageAngle(macd, 1) + " >= " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Last Histogram MACD is biger, then prelast: " + macd.Last().Histogram  + " > " + macd[macd.Count - 2].Histogram);
                Log.Information("Macd = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is not degree:  " + macdDegreeAverageAngle1 + " >= " + macdAverageAngleCondition);
                Log.Information("Macd Average of " + macdAverageAngleCount + " Angle is not degree:  " + macdDegreeAverageAngle + " >= " + macdAverageAngleCondition);
                Log.Information("Last Histogram MACD is lowest, then prelast: " + macd.Last().Histogram + " < " + macd[macd.Count - 2].Histogram);
                Log.Information("Macd = Long - false for: " + candleList.Figi);
                return false;
            }            
        }

        internal bool MacdFromLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start MACD FromLongSignal. Figi: " + candleList.Figi);
            List<MacdResult> macd = Mapper.MacdData(candleList, deltaPrice, macdFastPeriod, macdSlowPeriod, macdSignalPeriod);

            double macdDegreeAverageAngle = MacdDegreeAverageAngle(macd, macdAverageAngleCount);
            double macdDegreeAverageAngle1 = MacdDegreeAverageAngle(macd, 1);
            double macdHistogramDegreeAverageAngle1 = MacdHistogramDegreeAverageAngle(macd, 1);

            if (
                macd.Last().Macd < macd.Last().Signal
                || macd.Last().Histogram < 0
                || macdDegreeAverageAngle < macdAverageAngleCondition
                //|| macd.Last().Histogram < macd[macd.Count - 2].Histogram
                //|| MacdHistogramDegreeAverageAngle(macd, 1) < MacdHistogramDegreeAverageAngle(macd, 2)
                )
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is not degree:  " + macdDegreeAverageAngle1 + " < 0" );

                Log.Information("Macd Histogram Average Angle is not degree:  " + macdHistogramDegreeAverageAngle1 + " < " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Macd = FromLong - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Macd = " + macd.Last().Macd);
                Log.Information("Macd Histogram = " + macd.Last().Histogram);
                Log.Information("Macd Average Angle is degree:  " + macdDegreeAverageAngle1 + " >= 0");
                Log.Information("Macd Histogram Average Angle is degree:  " + MacdHistogramDegreeAverageAngle(macd, 1) + " < " + MacdHistogramDegreeAverageAngle(macd, 2));
                Log.Information("Macd = FromLong - false for: " + candleList.Figi);
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
