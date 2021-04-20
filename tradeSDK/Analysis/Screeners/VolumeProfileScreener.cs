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

namespace Analysis.Screeners
{
    public class VolumeProfileScreener : GetStocksHistory
    {
        MarketDataCollector dataCollector = new MarketDataCollector();

        bool PriceUpperBargaining(CandlesListProfile candlesListProfile)
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

        List<CandlesListProfile> MaxVolBargaining(List<CandlesListProfile> listCandlesListProfiles, int count)
        {
            var maxVol = listCandlesListProfiles.OrderByDescending(x => x.VolumeProfiles.OrderByDescending(y => y.Volume).FirstOrDefault());
            return maxVol.Take(count).ToList();
        }

        private static decimal AverageBargane(CandlesListProfile candlesListProfile)
        {
            var volProf = candlesListProfile.VolumeProfiles.OrderByDescending(x => x.Volume);
            var maxVol = volProf.FirstOrDefault();
            decimal bargainingPrice = (maxVol.LowerBound + maxVol.UpperBound) / 2;
            return bargainingPrice;
        }

        public CandlesListProfile VolumeProfileList(CandlesList candlesList, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
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
                    averageCandlesList = OpenCloseAverageMethod(candlesList);
                    break;

                case VolumeProfileMethod.HiLow:
                    averageCandlesList = HiLowAverageMethod(candlesList);
                    break;

                case VolumeProfileMethod.All:
                    averageCandlesList = FullAverageMethod(candlesList);
                    break;
            }

            List<VolumeProfile> volumeProfiles = new List<VolumeProfile>();

            for (int i = 0; i <= countVolumeProfile; i++)
            {
                decimal lowerBound = maxLow + (widthVolumeProfile * i);
                decimal upperBound = (maxLow + (widthVolumeProfile * i)) + widthVolumeProfile;
                VolumeProfile volumeProfile = new VolumeProfile(0, upperBound, lowerBound);
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
            CandlesListProfile volumeProfileList = new CandlesListProfile(candlesList.Figi, candlesList.Interval, countVolumeProfile,  volumeProfiles, candlesList.Candles);
            return volumeProfileList;
        }

        private List<VolumePrice> FullAverageMethod(CandlesList candlesList)
        {
            List<VolumePrice> averageCandlesList = new List<VolumePrice>();
            foreach (var item in candlesList.Candles)
            {
                decimal price = (item.High + item.Low + item.High + item.Low) / 4;
                decimal volume = item.Volume;
                VolumePrice volumePrice = new VolumePrice(price, volume);
                averageCandlesList.Add(volumePrice);
            }
            return averageCandlesList;
        }

        private List<VolumePrice> HiLowAverageMethod(CandlesList candlesList)
        {
            List<VolumePrice> averageCandlesList = new List<VolumePrice>();
            foreach (var item in candlesList.Candles)
            {
                decimal price = (item.High + item.Low) / 2;
                decimal volume = item.Volume;
                VolumePrice volumePrice = new VolumePrice(price, volume);
                averageCandlesList.Add(volumePrice);
            }
            return averageCandlesList;
        }

        private List<VolumePrice> OpenCloseAverageMethod (CandlesList candlesList)
        {
            List<VolumePrice> averageCandlesList = new List<VolumePrice>();
            foreach (var item in candlesList.Candles)
            {
                decimal price = (item.Open + item.Close) / 2;
                decimal volume = item.Volume;
                VolumePrice volumePrice = new VolumePrice(price, volume);
                averageCandlesList.Add(volumePrice);
            }
            return averageCandlesList;
        }

        //public async Task<List<Instrument>> NewMethod(List<Instrument> instrumentList, int countCandles, int countVolumeProfile, VolumeProfileMethod volumeProfileMethod)
        //{

        //    foreach (var item in instrumentList)
        //    {
        //        var candles = await dataCollector.GetCandlesAsync(item.Figi, CandleInterval.Day, countCandles);
        //        if (candles == null)
        //        {
        //            continue;
        //        }
        //        List<VolumeProfile> vps = VolumeProfileList(candles, 50, VolumeProfileMethod.All);
        //        var result = vps.OrderByDescending(x => x.Volume);
        //        var finalresult = result.FirstOrDefault();
        //        //if (finalresult.UpperBound < candles.Candles.Last().Close * 1.03m && finalresult.UpperBound > candles.Candles.Last().Close * 0.9m)
        //        decimal averageBound = (finalresult.UpperBound + finalresult.LowerBound) / 2;
        //        decimal procent = averageBound * 1.05M;
        //        if (averageBound < candles.Candles.Last().Close && procent >= candles.Candles.Last().Close)
        //        {
        //            Console.WriteLine(candles.Figi);
        //            Console.WriteLine(finalresult.UpperBound);
        //            Console.WriteLine(finalresult.LowerBound);
        //            Console.WriteLine(finalresult.Volume);
        //            Console.WriteLine(candles.Candles.Last().Close);
        //            Console.WriteLine("***");
        //            using (StreamWriter sw = new StreamWriter("tickers", true, System.Text.Encoding.Default))
        //            {
        //                sw.WriteLine(item.Ticker + " UpperBound: " + finalresult.UpperBound + " LowerBound: " + finalresult.LowerBound + " Volume: " + finalresult.Volume + " Close:" + candles.Candles.Last().Close);
        //                sw.WriteLine();
        //            }
        //        }
        //    }

        //    return instrumentList;
        //}


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
        internal decimal Price { get;  }
        internal decimal Volume { get;  }
        internal VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = volume;
        }

    }

    public class VolumeProfileList
    {
        public string Figi { get; }
        public int CountVolumeProfile { get; }
        public List<VolumeProfile> VolumeProfiles { get; }

        public VolumeProfileList(string figi, int countVolumeProfile, List<VolumeProfile> volumeProfiles)
        {
            Figi = figi;
            VolumeProfiles = volumeProfiles;
            CountVolumeProfile = countVolumeProfile;
        }
    }

 


    public enum VolumeProfileMethod
    {
        HiLow,
        OpenClose,
        All
    }
}

