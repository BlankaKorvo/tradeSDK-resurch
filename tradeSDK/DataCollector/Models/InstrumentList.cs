using System.Collections.Generic;

namespace DataCollector.Models
{
    public class InstrumentList
    {
        public int Total { get; }
        public List<Instrument> Instruments { get; }

        public InstrumentList(int total, List<Instrument> instruments)
        {
            Total = total;
            Instruments = instruments;
        }
    }
}