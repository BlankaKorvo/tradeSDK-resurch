using DataCollector;
using MarketDataModules;
using MarketDataModules.Models.Candles;
using ScreenerStocks.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace Analysis.Screeners
{
    public class VolumeProfileScreener : GetStocksHistory
    {
        MarketDataCollector dataCollector = new MarketDataCollector();
        IndicatorSignalsHelper indicatorSignalsHelper = new IndicatorSignalsHelper();

        public List<ProfileList> CreateProfilesList(List<CandlesList> listCandlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
        {
            List<ProfileList> profilesList = new List<ProfileList>();
            foreach (var item in listCandlesList)
            {
                if (item == null)
                    continue;
                profilesList.Add(VolumeProfileList(item, countVolumeProfile, volumeProfileMethod));
            }
            return profilesList;
        }

        bool PriceUpperBargaining(ProfileList candlesListProfile)
        {
            decimal bargainingPrice = AverageBargane(candlesListProfile);
            decimal price = candlesListProfile.Candles.Last().Close;
            if (price >= bargainingPrice)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public List<ProfileList> OrderVolBargaining(List<ProfileList> listCandlesListProfiles)
        {
            var maxVol = listCandlesListProfiles.OrderByDescending(x => (x.VolumeProfiles.Select(y => y).OrderByDescending(z => (z.VolumeRed + z.VolumeGreen)).FirstOrDefault())).ToList();
            return maxVol;
        }

        private static decimal AverageBargane(ProfileList candlesListProfile)
        {
            var volProf = candlesListProfile.VolumeProfiles.OrderByDescending(x => x.VolumeGreen);
            var maxVol = volProf.FirstOrDefault();
            decimal bargainingPrice = (maxVol.LowerBound + maxVol.UpperBound) / 2;
            return bargainingPrice;
        }

        public ProfileList VolumeProfileList(CandlesList candlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
        {
            if (candlesList == null)
            {
                return null;
            }

            decimal maxHi = MaxHi(candlesList);
            decimal maxLow = MaxLow(candlesList);
            decimal widthVolumeProfile = (maxHi - maxLow) / countVolumeProfile;

            List<VolumeProfile> volumeProfiles = new List<VolumeProfile>();

            for (int i = 0; i < countVolumeProfile; i++)
            {
                decimal lowerBound = maxLow + (widthVolumeProfile * i);
                decimal upperBound = (maxLow + (widthVolumeProfile * i)) + widthVolumeProfile;
                VolumeProfile volumeProfile = new VolumeProfile(0, 0, 0, upperBound, lowerBound);
                volumeProfiles.Add(volumeProfile);
            }
            ProfileList profileList = new ProfileList(candlesList.Figi, candlesList.Interval, countVolumeProfile , volumeProfiles, candlesList.Candles);

            foreach (var candle in candlesList.Candles)
            {
                decimal price = 0;
                switch (volumeProfileMethod)
                {
                    case VolumeProfileMethod.OpenClose:
                        price = OpenCloseAverageMethod(candle);
                        break;

                    case VolumeProfileMethod.HiLow:
                        price = HiLowAverageMethod(candle);
                        break;

                    case VolumeProfileMethod.All:
                        price = FullAverageMethod(candle);
                        break;
                }

                foreach (var profiles in volumeProfiles)
                {
                    if (price >= profiles.LowerBound && price < profiles.UpperBound)
                    {
                        profiles.CandlesCount++;
                        if (indicatorSignalsHelper.GreenCandle(candle))
                            profiles.VolumeGreen += candle.Volume;
                        else
                            profiles.VolumeRed += candle.Volume;
                    }
                }
            }
            return profileList;
        }

        private decimal FullAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.High + candleStructure.Low + candleStructure.High + candleStructure.Low) / 4;
            return price;
        }
        private decimal HiLowAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.High + candleStructure.Low) / 2;
            return price;
        }
        private decimal OpenCloseAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.Open + candleStructure.Close) / 2;
            return price;
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

    public enum VolumeProfileMethod
    {
        HiLow,
        OpenClose,
        All
    }
}

