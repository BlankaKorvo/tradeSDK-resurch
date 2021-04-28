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
        int BollingerBandsanglesCount = 3;
        internal bool BollingerBandsLongSignal(CandlesList candleList, decimal deltaPrice)
        {
            Log.Information("Start BollingerBands LongSignal. Figi: " + candleList.Figi);
            List<BollingerBandsResult> bollingerBands = Mapper.BollingerBandsData(candleList, deltaPrice);

            double bollingerBandsWidthDegreeAverageAngle = BollingerBandsWidthDegreeAverageAngle(bollingerBands, BollingerBandsanglesCount);
            double bollingerBandsWidthDegreeAverageAngle1 = BollingerBandsWidthDegreeAverageAngle(bollingerBands, 1);
            if (
                bollingerBandsWidthDegreeAverageAngle > 0
                &&
                bollingerBandsWidthDegreeAverageAngle1 > 0
                )
            {
                Log.Information("BollingerBands UpperBand = " + bollingerBands.Last().UpperBand + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands LowerBand = " + bollingerBands.Last().LowerBand + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands PercentB = " + bollingerBands.Last().PercentB + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands Sma = " + bollingerBands.Last().Sma + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands Width = " + bollingerBands.Last().Width + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands ZScore = " + bollingerBands.Last().ZScore + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands BollingerBands Width Degree Average Angle Count " + BollingerBandsanglesCount + " = " + bollingerBandsWidthDegreeAverageAngle);
                Log.Information("BollingerBands BollingerBands Width Degree Average Angle Count 1 = " + bollingerBandsWidthDegreeAverageAngle1);
                Log.Information("BollingerBands = Long - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("BollingerBands UpperBand = " + bollingerBands.Last().UpperBand + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands LowerBand = " + bollingerBands.Last().LowerBand + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands PercentB = " + bollingerBands.Last().PercentB + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands Sma = " + bollingerBands.Last().Sma + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands Width = " + bollingerBands.Last().Width + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands ZScore = " + bollingerBands.Last().ZScore + " " + bollingerBands.Last().Date);
                Log.Information("BollingerBands BollingerBands Width Degree Average Angle Count " + BollingerBandsanglesCount + " = " + bollingerBandsWidthDegreeAverageAngle);
                Log.Information("BollingerBands BollingerBands Width Degree Average Angle Count 1 = " + bollingerBandsWidthDegreeAverageAngle1);
                Log.Information("BollingerBands = Long - false for: " + candleList.Figi);
                return false;
            }
        }
        double BollingerBandsWidthDegreeAverageAngle(List<BollingerBandsResult> bollingerBands, int anglesCount)
        {
            Log.Information("Start BollingerBandsWidthDegreeAverageAngle method.");
            List<BollingerBandsResult> skipbollingerBands = bollingerBands.Skip(bollingerBands.Count - (anglesCount + 1)).ToList();
            List<decimal?> values = new List<decimal?>();
            foreach (var item in skipbollingerBands)
            {
                values.Add(item.Width);
                Log.Information("Bollinger Bands Width for Degree Average Angle: " + item.Date + " " + item.Width);
            }
            Log.Information("Stop BollingerBandsWidthDegreeAverageAngle method.");
            return DeltaDegreeAngle(values);
        }
    }
}
