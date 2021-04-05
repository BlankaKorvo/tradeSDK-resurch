using System.Collections.Generic;

namespace DataCollector.Models
{
    public interface ICandlesList
    {   
        public string Figi { get; }
        public CandleInterval Interval { get; }
        public List<CandleStructure> Candles { get; }
    }
    public class CandlesList
    {
        public string Figi { get; }
        public CandleInterval Interval { get; }
        public List<CandleStructure> Candles { get; }

        public CandlesList(string figi, CandleInterval interval, List<CandleStructure> candles)
        {
            Figi = figi;
            Interval = interval;
            Candles = candles;
        }
    }
}