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
    partial class Signal : IndicatorSignalsHelper
    {
        int smaLookbackPeriod = 8;
        const decimal smaPriceDeltaCount = 0.15M;
        internal bool SmaLongSignal(CandleList candleList, decimal deltaPrice)
        {
            Log.Information("Start Sma LongSignal. Figi: " + candleList.Figi);
            List<SmaResult> sma = Serialization.SmaData(candleList, deltaPrice, smaLookbackPeriod);
            decimal? smaPriceDelta = 100 - (sma.Last().Sma * 100 / deltaPrice); //Насколько далеко убежала цена от Sma
            if (
                smaPriceDelta < smaPriceDeltaCount
               )
            {
                Log.Information("Checking for the absence of a gap via SMA");
                Log.Information("smaPriceDelta = " + smaPriceDelta);
                Log.Information("smaPriceDeltaCount = " + smaPriceDeltaCount);
                Log.Information("Should be: smaPriceDelta < smaPriceDeltaCount");
                Log.Information("Sma = Long - true for: " + candleList.Figi);
                return true; 
            }
            else
            {
                Log.Information("Checking for the absence of a gap via SMA");
                Log.Information("smaPriceDelta = " + smaPriceDelta);
                Log.Information("smaPriceDeltaCount = " + smaPriceDeltaCount);
                Log.Information("Should be: smaPriceDelta < smaPriceDeltaCount");
                Log.Information("Sma = Long - falce for: " + candleList.Figi);
                return false; 
            }
        }


    }
}
