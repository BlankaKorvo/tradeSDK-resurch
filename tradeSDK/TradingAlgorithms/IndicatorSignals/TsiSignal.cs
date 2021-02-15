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
    class TsiSignal : IndicatorSignalsHelper
    {
        int lookbackPeriod = 8;
        int averageAngleCount = 2;
        int fromLongAverageAngleCount = 2;
        //internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        //{
        //    List<AdxResult> tsi = Serialization.TsiData(candleList, deltaPrice, lookbackPeriod);
        //}
    }
}
