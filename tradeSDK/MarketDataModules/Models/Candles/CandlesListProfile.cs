using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataModules.Models.Candles
{
    public class CandlesListProfile : CandlesList
    {
        public int CountVolumeProfile { get; }
        public List<VolumeProfile> VolumeProfiles { get; }
        public CandlesListProfile(string figi, CandleInterval interval, int countVolumeProfile, List<VolumeProfile> volumeProfiles, List<CandleStructure> candles) : base(figi, interval, candles)
        {
            VolumeProfiles = volumeProfiles;
            CountVolumeProfile = countVolumeProfile;
        }
    }
    public class VolumeProfile
    {
        public decimal Volume { get; set; }
        public decimal UpperBound { get; }
        public decimal LowerBound { get; }
        public VolumeProfile(decimal volume, decimal upperBound, decimal lowerBound)
        {
            Volume = volume;
            UpperBound = upperBound;
            LowerBound = lowerBound;

        }
    }
}
