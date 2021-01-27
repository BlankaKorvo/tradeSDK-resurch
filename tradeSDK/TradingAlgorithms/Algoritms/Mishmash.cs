using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using TradingAlgorithms.IndicatorSignals;

namespace TradingAlgorithms.Algoritms
{
    public class Mishmash
    {
        DpoSignal dpoSignal = new DpoSignal();
        SuperTrendSignal superTrendSignal = new SuperTrendSignal();
        IchimokuSignal ichimokuSignal = new IchimokuSignal();

        //Передаваемые при создании объекта параметры
        public CandleList candleList { get; set; }
        public decimal deltaPrice { get; set; }

        //Тюнинг индикаторов
        private int dpoPeriod = 20;


        public bool Long()
        {
            if (dpoSignal.LongSignal(candleList, deltaPrice)
                && superTrendSignal.LongSignal(candleList, deltaPrice)
                && ichimokuSignal.LongSignal(candleList, deltaPrice))
            { return true; }
            else 
            { return false; }
        }
        public bool FromLong()
        {
            if ((dpoSignal.FromLongSignal(candleList, deltaPrice) 
                    && superTrendSignal.FromLongSignal(candleList, deltaPrice))
                || ichimokuSignal.FromLongSignal(candleList, deltaPrice))
            { return true; }
            else
            { return false; }
        }
    }
}
