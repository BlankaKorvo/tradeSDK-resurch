using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataModules.Models.Candles
{
    public class CandlesProfileList : CandlesList
    {
        public int CountVolumeProfile { get; }
        public List<VolumeProfile> VolumeProfiles { get; }
        public CandlesProfileList(string figi, CandleInterval interval, int countVolumeProfile, List<VolumeProfile> volumeProfiles, List<CandleStructure> candles) : base(figi, interval, candles)
        {
            VolumeProfiles = volumeProfiles;
            CountVolumeProfile = countVolumeProfile;
        }
    }
    public class VolumeProfile
    {
        public decimal VolumeGreen { get; set; }
        public decimal VolumeRed { get; set; }
        public int CandlesCount { get; set; }
        public decimal UpperBound { get; }
        public decimal LowerBound { get; }
        public VolumeProfile(decimal volumeGreen, decimal volumeRed, int candlesCount, decimal upperBound, decimal lowerBound)
        {
            VolumeGreen = volumeGreen;
            VolumeRed = volumeRed;
            CandlesCount = candlesCount;
            UpperBound = upperBound;
            LowerBound = lowerBound;
        }
    }
    public class CandleStructureAvergage : CandleStructure
    {
        public decimal Price { get; }
        public CandleStructureAvergage(decimal price, decimal open, decimal close, decimal high, decimal low, decimal volume, DateTime time, CandleInterval interval, string figi) : base(open, close, high, low, volume, time, interval, figi )
        {
            Price = price;
        }

    }
}
