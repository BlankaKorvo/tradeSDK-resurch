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
        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<ObvResult> obvFirst = Serialization.ObvData(candleList, deltaPrice, lookbackPeriodFirst);
            List<ObvResult> obvSecond = Serialization.ObvData(candleList, deltaPrice, lookbackPeriodSecond);

            if (
                obvFirst.Last().ObvSma > obvSecond.Last().ObvSma
                &&
                obvFirst.Last().Obv > obvFirst.Last().ObvSma              
               )

            {
                Log.Information("Start ObvSignal");
                Log.Information("Obv = " + obvFirst.Last().Obv);
                Log.Information("ObvSmaDenominator = " + obvFirst.Last().ObvSmaDenominator);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst  + " ) = " + obvFirst.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodSecond + " ) = " + obvSecond.Last().ObvSma);
                Log.Information("Sma(Obv, " + lookbackPeriodFirst + " ) must be more then Sma(Obv, " + lookbackPeriodSecond + " )");
                Log.Information("and");
                Log.Information("Obv must be more then Sma(Obv, " + lookbackPeriodFirst + " )");
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
                Log.Information("ObvSignal = Long - false");
                Log.Information("Stop ObvSignal");
                return false;
            }
        }
    }
}
