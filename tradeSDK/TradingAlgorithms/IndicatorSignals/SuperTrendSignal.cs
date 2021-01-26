using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;

namespace TradingAlgorithms.IndicatorSignals
{
    internal class SuperTrendSignal

    {
        //int superTrandPeriod = 20;
        //int superTrandSensitive = 2;
        internal bool LongSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod, int superTrandSensitive)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod,  superTrandSensitive);
            if (superTrand.Last().UpperBand == null)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        internal bool ShortgSignal(CandleList candleList, decimal deltaPrice, int superTrandPeriod, int superTrandSensitive)
        {
            List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, deltaPrice, superTrandPeriod, superTrandSensitive);
            if (superTrand.Last().LowerBand == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
