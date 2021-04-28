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
        int tsiLookbackPeriod = 8;
        int tsiAverageAngleCount = 2;
        int tsiFromLongAverageAngleCount = 2;
        //internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        //{
        //    List<AdxResult> tsi = Serialization.TsiData(candleList, deltaPrice, lookbackPeriod);
        //}
    }
}
