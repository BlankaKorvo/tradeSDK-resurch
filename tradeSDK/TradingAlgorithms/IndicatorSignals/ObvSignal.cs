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
    class ObvSignal : IndicatorSignalsHelper
    {
        int lookbackPeriodFirst = 4;
        int lookbackPeriodSecond = 17;
        int anglesCount = 3;

        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<ObvResult> obvFirst = Serialization.ObvData(candleList, deltaPrice, lookbackPeriodFirst);
            List<ObvResult> obvSecond = Serialization.ObvData(candleList, deltaPrice, lookbackPeriodSecond);

            if (
                obvFirst.Last().ObvSma > obvSecond.Last().ObvSma
                &&
                obvFirst.Last().Obv > obvFirst.Last().ObvSma
                &&
                //углы lookbackPeriodFirst
                ObvDegreeAverageAngle(obvFirst, anglesCount, obv.obv) > 0
                &&
                ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSma) > 0
                &&
                ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSmaDenominator) > 0
                &&
                //углы lookbackPeriodSecond
                ObvDegreeAverageAngle(obvSecond, anglesCount, obv.obv) > 0
                &&
                ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSma) > 0
                &&
                ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSmaDenominator) > 0
               )

            {
                Log.Information("Start ObvSignal");
                Log.Information("Obv = " + obvFirst.Last().Obv);
                Log.Information("ObvSmaDenominator = " + obvFirst.Last().ObvSmaDenominator);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst + " ) = " + obvFirst.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodSecond + " ) = " + obvSecond.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst + " ) must be more then Sma(Obv, " + lookbackPeriodSecond + " )");
                Log.Information("and");
                Log.Information("Obv must be more then Sma(Obv, " + lookbackPeriodFirst + " )");
                Log.Information("Obv degree average angle obv( lookback period = " + lookbackPeriodFirst+ "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.obv));
                Log.Information("Obv degree average angle obv sma( lookback period = " + lookbackPeriodFirst + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSma));
                Log.Information("Obv degree average angle Obv Sma denominator( lookback period = " + lookbackPeriodFirst + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSmaDenominator));
                Log.Information("Obv degree average angle obv( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.obv));
                Log.Information("Obv degree average angle obv sma( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSma));
                Log.Information("Obv degree average angle Obv Sma denominator( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSmaDenominator));
                Log.Information("ObvSignal = Long - true");
                Log.Information("Stop ObvSignal");
                return true;
            }
            else
            {
                Log.Information("Start ObvSignal");
                Log.Information("Obv = " + obvFirst.Last().Obv);
                Log.Information("ObvSmaDenominator = " + obvFirst.Last().ObvSmaDenominator);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst + " ) = " + obvFirst.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodSecond + " ) = " + obvSecond.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst + " ) must be more then Sma(Obv, " + lookbackPeriodSecond + " )");
                Log.Information("and");
                Log.Information("Obv must be more then Sma(Obv, " + lookbackPeriodFirst + " )");
                Log.Information("Obv degree average angle obv( lookback period = " + lookbackPeriodFirst + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.obv));
                Log.Information("Obv degree average angle obv sma( lookback period = " + lookbackPeriodFirst + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSma));
                Log.Information("Obv degree average angle Obv Sma denominator( lookback period = " + lookbackPeriodFirst + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvFirst, anglesCount, obv.ObvSmaDenominator));
                Log.Information("Obv degree average angle obv( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.obv));
                Log.Information("Obv degree average angle obv sma( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSma));
                Log.Information("Obv degree average angle Obv Sma denominator( lookback period = " + lookbackPeriodSecond + "; anglesCount = " + anglesCount + " ) = " + ObvDegreeAverageAngle(obvSecond, anglesCount, obv.ObvSmaDenominator));
                Log.Information("ObvSignal = Long - false");
                Log.Information("Stop ObvSignal");
                return false;
            }
            double ObvDegreeAverageAngle(List<ObvResult> ObvValue, int anglesCount, obv obvLine)
            {
                List<ObvResult> skipObv = ObvValue.Skip(ObvValue.Count - (anglesCount + 1)).ToList();
                List<decimal?> values = new List<decimal?>();
                foreach (var item in skipObv)
                {
                    switch (obvLine)
                    {
                        case obv.obv:
                            values.Add(item.Obv);
                            Log.Information("Obv degree average of " + anglesCount + " angles: " + item.Date + " " + item.Obv);
                            break;
                        case obv.ObvSma:
                            values.Add(item.ObvSma);
                            Log.Information("ObvSma degree average of " + anglesCount + " angles: " + item.Date + " " + item.ObvSma);
                            break;
                        case obv.ObvSmaDenominator:
                            values.Add(item.ObvSmaDenominator);
                            Log.Information("ObvSmaDenominator degree average of " + anglesCount + " angles: " + item.Date + " " + item.ObvSmaDenominator);
                            break;
                    }
                }
                return DeltaDegreeAngle(values);
            }

        }
        enum obv
        {
            obv,
            ObvSma,
            ObvSmaDenominator
        }
    }
}
