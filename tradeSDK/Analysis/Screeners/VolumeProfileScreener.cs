using MarketDataModules;
using ScreenerStocks.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis.Screeners
{
    public class VolumeProfileScreener : GetStocksHistory
    {
        public List<VolumeProfile> MaxVol(CandlesList candlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
        {
            if (candlesList == null)
            {
                return null;
            }

            decimal maxHi = MaxHi(candlesList);
            decimal maxLow = MaxLow(candlesList);
            decimal widthVolumeProfile = (maxHi - maxLow) / countVolumeProfile;


            List<VolumePrice> averageCandlesList = new List<VolumePrice>();

            switch (volumeProfileMethod)
            {
                case VolumeProfileMethod.OpenClose:
                    foreach (var item in candlesList.Candles)
                    {
                        VolumePrice volumePrice = new VolumePrice();
                        volumePrice.Price = (item.Open + item.Close) / 2;
                        volumePrice.Volume = item.Volume;
                        averageCandlesList.Add(volumePrice);
                    }
                    break;

                case VolumeProfileMethod.HiLow:
                    foreach (var item in candlesList.Candles)
                    {
                        VolumePrice volumePrice = new VolumePrice();
                        volumePrice.Price = (item.High + item.Low) / 2;
                        volumePrice.Volume = item.Volume;
                        averageCandlesList.Add(volumePrice);
                    }
                    break;

                case VolumeProfileMethod.All:
                    foreach (var item in candlesList.Candles)
                    {
                        VolumePrice volumePrice = new VolumePrice();
                        volumePrice.Price = (item.High + item.Low + item.High + item.Low) / 4;
                        volumePrice.Volume = item.Volume;
                        averageCandlesList.Add(volumePrice);
                    }
                    break;
            }

            List<VolumeProfile> volumeProfiles = new List<VolumeProfile>();

            for (int i = 0; i <= countVolumeProfile; i++)
            {

                VolumeProfile volumeProfile = new VolumeProfile() { LowerBound = maxLow + (widthVolumeProfile * i), UpperBound = (maxLow + (widthVolumeProfile * i)) + widthVolumeProfile, Volume = 0 };
                volumeProfiles.Add(volumeProfile);
            }

            //List<VolumeProfile> volumeProfile = new List<VolumeProfile>();

            foreach (var candle in averageCandlesList)
            {
                foreach (var profiles in volumeProfiles)
                {
                    if (candle.Price >= profiles.LowerBound && candle.Price < profiles.UpperBound)
                    {
                        profiles.Volume += candle.Volume;
                    } 
                }
            }
            return volumeProfiles;
        }

        private decimal MaxLow(CandlesList candlesList)
        {
            return candlesList.Candles.Select(x => x.Low).Min();
        }

        private decimal MaxHi(CandlesList candlesList)
        {
            return candlesList.Candles.Select(x => x.High).Max();
        }
    }

    internal class VolumePrice
    {
        internal decimal Price { get; set; }
        internal decimal Volume { get; set; }
    }

    internal class VolumeProfileList
    {
        string Figi { get; }
        List<VolumeProfile> VolumeProfiles { get;}
    }

 
    public class VolumeProfile 
    {
        public decimal Volume { get; set; }
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
    }

    public enum VolumeProfileMethod
    {
        HiLow,
        OpenClose,
        All
    }
}

