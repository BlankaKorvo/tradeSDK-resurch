using DataCollector;
using MarketDataModules;
using MarketDataModules.Models.Candles;
using ScreenerStocks.Helpers;
using Serilog;
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
        //MarketDataCollector dataCollector = new MarketDataCollector();
        IndicatorSignalsHelper indicatorSignalsHelper = new IndicatorSignalsHelper();

        public List<CandlesProfileList> CreateProfilesList(List<CandlesList> listCandlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
        {
            List<CandlesProfileList> profilesList = new List<CandlesProfileList>();
            foreach (var item in listCandlesList)
            {
                if (item == null)
                    continue;
                profilesList.Add(VolumeProfileList(item, countVolumeProfile, volumeProfileMethod));
            }
            return profilesList;
        }

        bool PriceUpperBargaining(CandlesProfileList candlesProfileList)
        {
            decimal bargainingPrice = AverageBargane(candlesProfileList);
            decimal price = candlesProfileList.Candles.Last().Close;
            if (price >= bargainingPrice)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        VolumeProfile GetVolumeProfileMaxVolPrice(CandlesProfileList candlesProfileList) // Вытаскивает VolumeProfile с самым большим объёмом
        {
            return candlesProfileList.VolumeProfiles.OrderByDescending(x => x.VolumeGreen + x.VolumeRed).FirstOrDefault();
        }

        decimal MaxVolPrice(CandlesProfileList candlesProfileList)  // Расчитывает сумму оборота из самого объёмного VolumeProfile
        {
            var volumeProfile = candlesProfileList.VolumeProfiles.OrderByDescending(x => x.VolumeGreen + x.VolumeRed).FirstOrDefault();
            return volumeProfile.VolumeGreen + volumeProfile.VolumeRed;
        }

        public List<CandlesProfileList> OrderVolBargaining(List<CandlesProfileList> listCandlesListProfiles) // Сортировка инструметов по объему самого объёмного VolumeProfile
        {
            var maxVol = listCandlesListProfiles.OrderByDescending(y => MaxVolPrice(y)).ToList();
            return maxVol;
        }

        public List<CandlesProfileList> BargainingOnPrice(List<CandlesProfileList> listCandlesListProfiles, decimal percent) // Сортировка инструметов по объему самого объёмного VolumeProfile
        {
            List<CandlesProfileList> result = 
                (from x in listCandlesListProfiles where 
                 AverageBargane(x) <= x.Candles.Last().Close * (percent / 100 + 1)
                 && 
                 AverageBargane(x) >= x.Candles.Last().Close * (1 - (percent / 100)) 
                 select x).ToList();
            return result;
        }

        public decimal RevWeightGreen(VolumeProfile VolumeProfile) //процент веса в общем объеме
        {
            decimal result = VolumeProfile.VolumeGreen * 100 / (VolumeProfile.VolumeRed + VolumeProfile.VolumeGreen);
            return result;
        }

        decimal AverageBargane(CandlesProfileList candlesListProfile) // Среднее значение границ горизонтального канала прторговки (VolumeProfile:  UpperBound & LowerBound )
        {
            Log.Information("Start AverageBargane. Figi: " + candlesListProfile.Figi);
            var volProf = candlesListProfile.VolumeProfiles.OrderByDescending(x => x.VolumeGreen + x.VolumeRed);
            var maxVol = volProf.FirstOrDefault();
            decimal bargainingPrice = (maxVol.LowerBound + maxVol.UpperBound) / 2;
            Log.Information("Stop AverageBargane. Figi: " + candlesListProfile.Figi);
            return bargainingPrice;

        }

        public CandlesProfileList VolumeProfileList(CandlesList candlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod) //mapping from candlesList to CandlesProfileList
        {
            if (candlesList == null || candlesList.Candles.Count == 0)
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
            CandlesProfileList profileList = new CandlesProfileList(candlesList.Figi, candlesList.Interval, countVolumeProfile , volumeProfiles, candlesList.Candles);

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
                        if (indicatorSignalsHelper.IsCandleGreen(candle))
                            profiles.VolumeGreen += candle.Volume;
                        else
                            profiles.VolumeRed += candle.Volume;
                    }
                }
            }
            return profileList;
        }

        decimal FullAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.High + candleStructure.Low + candleStructure.High + candleStructure.Low) / 4;
            return price;
        }
        decimal HiLowAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.High + candleStructure.Low) / 2;
            return price;
        }
        decimal OpenCloseAverageMethod(CandleStructure candleStructure)
        {
            decimal price = (candleStructure.Open + candleStructure.Close) / 2;
            return price;
        }

        decimal MaxLow(CandlesList candlesList)
        {
            Log.Information("Start MaxLow. Figi: " + candlesList.Figi);
            decimal result = candlesList.Candles.Select(x => x.Low).DefaultIfEmpty().Min();
            Log.Information("Stop MaxLow. Figi: " + candlesList.Figi);
            return result;
        }

        decimal MaxHi(CandlesList candlesList)
        {
           Log.Information("Start MaxHigh. Figi: " + candlesList.Figi);
            decimal result = candlesList.Candles.Select(x => x.High).DefaultIfEmpty().Max();
            Log.Information("Stop MaxHigh. Figi: " + candlesList.Figi);
            return result;
        }
    }

    public enum VolumeProfileMethod
    {
        HiLow,
        OpenClose,
        All
    }
}

