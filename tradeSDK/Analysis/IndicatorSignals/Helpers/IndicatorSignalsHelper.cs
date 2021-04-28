using MarketDataModules;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingAlgorithms.IndicatorSignals.Helpers
{
    public class IndicatorSignalsHelper
    {
        public double DeltaDegreeAngle(List<decimal?> values)
        {
            Log.Information("Start DeltaDegreeAngle");
            var countDelta = values.Count;
            double summ = 0;
            for (int i = 1; i < countDelta; i++)
            {
                double deltaLeg = Convert.ToDouble(values[i] - values[i - 1]);
                double legDifference = Math.Atan(deltaLeg);
                double angle = legDifference * (180 / Math.PI);
                Log.Information("Angle: " + angle.ToString());
                summ += angle;
            }
            double averageAngles = summ / (countDelta - 1);
            Log.Information("Average Angles: " + averageAngles.ToString());
            Log.Information("Stop DeltaDegreeAngle");
            return averageAngles;
        }

        internal double DeltaDegreeAngle(List<decimal?> values, int anglesCount)
        {
            Log.Information("Start DeltaDegreeAngle");
            List<decimal?> calculatedValues = values.Skip(values.Count - (anglesCount + 1)).ToList();
            var countDelta = calculatedValues.Count;
            double summ = 0;
            for (int i = 1; i < countDelta; i++)
            {
                double deltaLeg = Convert.ToDouble(calculatedValues[i] - calculatedValues[i - 1]);
                double legDifference = Math.Atan(deltaLeg);
                double angle = legDifference * (180 / Math.PI);
                Log.Information("Angle: " + angle.ToString());
                summ += angle;
            }
            double averageAngles = summ / (countDelta - 1);
            Log.Information("Average Angles: " + averageAngles.ToString());
            Log.Information("Stop DeltaDegreeAngle");
            return averageAngles;
        }

        internal bool GreenCandle(CandleStructure candleStructure)
        {
            if (candleStructure.Open <= candleStructure.Close)
                return true;
            else
                return false;
        }
    }
}
