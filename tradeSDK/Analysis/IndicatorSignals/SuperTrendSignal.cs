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
    partial class Signal
    {
        int superTrandPeriod = 20;
        int superTrandSensitive = 2;

        //int superTrandPeriod = 20;
        //int superTrandSensitive = 2;
        internal bool SuperTrendLongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<SuperTrendResult> superTrand = Mapper.SuperTrendData(candleList, deltaPrice, superTrandPeriod,  superTrandSensitive);
            if (superTrand.Last().UpperBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand = Long - true for: " + candleList.Figi);
                return true;
            }
            else 
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand = Long - false for: " + candleList.Figi);
                return false;
            }
        }
        internal bool SuperTrendFromLongSignal(CandleList candleList, decimal deltaPrice)
        {
            List<SuperTrendResult> superTrand = Mapper.SuperTrendData(candleList, deltaPrice, superTrandPeriod, superTrandSensitive);
            if (superTrand.Last().LowerBand == null)
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().UpperBand.ToString());
                Log.Information("Super Trand = FromLong - true for: " + candleList.Figi);
                return true;
            }
            else
            {
                Log.Information("Super Trand Period = " + superTrandPeriod);
                Log.Information("Super Trand Sensitive = " + superTrandSensitive);
                Log.Information("super Trand LowerBand = " + superTrand.Last().LowerBand.ToString());
                Log.Information("Super Trand = FromLong - false for: " + candleList.Figi);
                return false;
            }
        }
    }
}
