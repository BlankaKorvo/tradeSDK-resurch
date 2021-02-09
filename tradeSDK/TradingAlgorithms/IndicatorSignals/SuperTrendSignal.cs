using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Skender.Stock.Indicators;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffData;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class SuperTrendSignal
    {
        const int _superTrandPeriod = 20;
        const int _superTrandSensitive = 2;

        //int superTrandPeriod = 20;
        //int superTrandSensitive = 2;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod = _superTrandPeriod, int superTrandSensitive = _superTrandSensitive)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod,  superTrandSensitive);
            if (superTrand.Last().UpperBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand = Long - true");
                return true;
            }
            else 
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand = Long - false");
                return false;
            }
        }
        internal bool FromLongSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod = _superTrandPeriod, int superTrandSensitive = _superTrandSensitive)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod, superTrandSensitive);
            if (superTrand.Last().LowerBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand = FromLong - true");
                return true;
            }
            else
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand = FromLong - false");
                return false;
            }
        }
    }
}
