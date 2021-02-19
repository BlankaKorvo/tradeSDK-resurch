using Serilog;
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
        MacdSignal macdSignal = new MacdSignal();
        BollingerBandsSignal bollingerBandsSignal = new BollingerBandsSignal();
        AroonSignal aroonSignal = new AroonSignal();
        AdxSignal adxSignal = new AdxSignal();
        SmaSignal smaSignal = new SmaSignal();
        ObvSignal obvSignal = new ObvSignal();


        //Передаваемые при создании объекта параметры
        public CandleList candleList { get; set; }
        public decimal deltaPrice { get; set; }

        //Тюнинг индикаторов


        public bool Long()
        {
            if (
                
                dpoSignal.LongSignal(candleList, deltaPrice)
                &&
                macdSignal.LongSignal(candleList, deltaPrice)
                &&
                aroonSignal.LongSignal(candleList, deltaPrice)
                &&
                adxSignal.LongSignal(candleList, deltaPrice)               
                &&
                obvSignal.LongSignal(candleList, deltaPrice)
                //Проверка на отсутствие боковика
                &&
                bollingerBandsSignal.LongSignal(candleList, deltaPrice)
                
                //Проверка на отсутсвие гэпа
                &&
                smaSignal.LongSignal(candleList, deltaPrice)
                )
            {
                Log.Information("Mishmash Algoritms: Long - true " + candleList.Figi);
                return true; 
            }
            else 
            {
                Log.Information("Mishmash Algoritms: Long - false " + candleList.Figi);
                return false;
            }
        }
        public bool FromLong()
        {
            if (
                macdSignal.FromLongSignal(candleList, deltaPrice)
                ||
                adxSignal.FromLongSignal(candleList, deltaPrice)
                ||
                aroonSignal.FromLongSignal(candleList, deltaPrice)
                )
            {
                Log.Information("Mishmash Algoritms: FromLong - true " + candleList.Figi);
                return true; 
            }
            else
            {
                Log.Information("Mishmash Algoritms: FromLong - false " + candleList.Figi);
                return false; 
            }
        }
    }
}
