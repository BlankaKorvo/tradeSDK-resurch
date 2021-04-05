using System;

namespace DataCollector.Models
{
    public class CandleStructure
    {
        public decimal Open { get; }
        public decimal Close { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Volume { get; }
        public DateTime Time { get; }
        public CandleInterval Interval { get; }
        public string Figi { get; }


        //public CandlePayload(
        //    decimal open,
        //    decimal close,
        //    decimal high,
        //    decimal low,
        //    decimal volume,
        //    DateTime time,
        //    CandleInterval interval,
        //    string figi)
        //{
        //    Open = open;
        //    Close = close;
        //    High = high;
        //    Low = low;
        //    Volume = volume;
        //    Time = time;
        //    Interval = interval;
        //    Figi = figi;
        //}

        //public override string ToString()
        //{
        //    return $"{nameof(Figi)}: {Figi}, {nameof(Interval)}: {Interval}, {nameof(Time)}: {Time}, {nameof(Open)}: {Open}, {nameof(Close)}: {Close}, {nameof(High)}: {High}, {nameof(Low)}: {Low}, {nameof(Volume)}: {Volume}";
        //}
    }
}
