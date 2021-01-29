using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Skender.Stock.Indicators;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class SuperTrendSignal
    {
        //int superTrandPeriod = 20;
        //int superTrandSensitive = 2;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod = 20, int superTrandSensitive = 2)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod,  superTrandSensitive);
            if (superTrand.Last().UpperBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand Status = Buy");
                return true;
            }
            else 
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand Status = Sell");
                return false;
            }
        }
        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod = 20, int superTrandSensitive = 2)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod, superTrandSensitive);
            if (superTrand.Last().LowerBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand Status = Sell");
                return true;
            }
            else
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand Status = Buy");
                return false;
            }
        }
    }
}
