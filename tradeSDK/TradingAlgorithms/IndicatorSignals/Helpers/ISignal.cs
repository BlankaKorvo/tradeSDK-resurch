using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace TradingAlgorithms.IndicatorSignals
{
    interface ISignal
    {
        bool AdxLongSignal(CandleList candleList, decimal deltaPrice);
        bool AdxFromLongSignal(CandleList candleList, decimal deltaPrice);
    }
}
