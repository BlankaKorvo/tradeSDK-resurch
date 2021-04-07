using MarketDataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingAlgorithms.IndicatorSignals
{
    interface ISignal
    {
        bool AdxLongSignal(CandlesList candleList, decimal deltaPrice);
        bool AdxFromLongSignal(CandlesList candleList, decimal deltaPrice);
    }
}
