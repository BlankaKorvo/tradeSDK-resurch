using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffData;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class MACDSignal
    {
        const int _superTrandPeriod = 20;
        const int _superTrandSensitive = 2;

        //int superTrandPeriod = 20;
        //int superTrandSensitive = 2;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<MacdResult> superTrand = Serialization.MacdData(candleList, deltaPrice);
            //if (superTrand.Last().UpperBand == null)
            //{
            //    Log.Information("Super Trand Period = " + superTrandPeriod);
            //    Log.Information("Super Trand Sensitive = " + superTrandSensitive);
            //    Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
            //    Log.Information("Super Trand Status = Buy");
            //    return true;
            //}
            //else
            //{
            //    Log.Information("Super Trand Period = " + superTrandPeriod);
            //    Log.Information("Super Trand Sensitive = " + superTrandSensitive);
            //    Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
            //    Log.Information("Super Trand Status = Sell");
            //    return false;
            //}
        }
    }
}
