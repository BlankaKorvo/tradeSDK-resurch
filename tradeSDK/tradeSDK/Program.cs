using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;



namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Market market = new Market();
            SandboxContext context = new Auth().GetSanboxContext();
            //Serialization ser = new Serialization();

            int dpoPeriod = 20;
            int emaPeriod = 10;

            int superTrandPeriod = 20;
            int superTrandSensitive = 2;
            decimal EmaPriceDeltaCondition = 0.12M;
            //decimal DeltaThreeDpoLongCondition = 1;
            //decimal DeltaThreeDpoFromLongCondition = 0;
            decimal deltaAngleFourDpoLongCondition = 30;
            decimal deltaAngleFourDpoFromLongCondition = -50;

            decimal longLastDpoCondition = 0;
            decimal fromLongDpoCondition = 0;


            while (true)
            {
                try
                {
                    int count = 0;
                    var figi = "BBG000BVPV84";
                    var date = DateTime.Now;
                    var candleInterval = CandleInterval.FiveMinutes;

                    CandleList candleList = await market.GetCandleByFigi(context, figi, candleInterval, date);
                    //var list1 = new List<A>() { new A { SomeProp1 = 1, SomeProp2 = "A" }, new A { SomeProp1 = 2, SomeProp2 = "B" } };

                    CandleList first = await market.GetCandleByFigi(context, figi, candleInterval, date);
                    CandleList second = await market.GetCandleByFigi(context, figi, candleInterval, date.AddDays(-1));


                    var orderbook = await context.MarketOrderbookAsync(figi, 1);
                    if (orderbook.Asks.Count == 0)
                    {
                        Console.WriteLine(DateTime.Now + "  биржа не фурычит");
                        Thread.Sleep(200);
                        continue;
                    }               

                    CandlePayload lastCandle = candleList.Candles.Last();
                    CandlePayload preLastCandle = candleList.Candles[candleList.Candles.Count - 2];
                    CandlePayload prePreLastCandle = candleList.Candles[candleList.Candles.Count - 3];

                    var ask = orderbook.Asks.Last().Price;
                    var bid = orderbook.Bids.Last().Price;

                    var lastPrice = orderbook.LastPrice;
                    var closePrice = orderbook.ClosePrice;

                    var deltaPrice = (ask + bid) / 2;

                    var deltaPriceOneLast = (lastCandle.Close + lastCandle.Open + lastCandle.High + lastCandle.Low) / 4;
                    var deltaPraiceTwoLast = (lastCandle.Close + lastCandle.Open + lastCandle.High + lastCandle.Low + preLastCandle.Close + preLastCandle.Open + preLastCandle.High + preLastCandle.Low) / 8;
                                        
                    //int barsback = dpoPeriod / 2 + 1;
                    List<SmaResult> smaForDpo = Serialization.SmaData(candleList, dpoPeriod, deltaPrice);
                    List<EmaResult> ema = Serialization.EmaData(candleList, emaPeriod, deltaPrice);
                    //SmaResult lastma = sma[sma.Count - barsback -1];
                    //decimal? lastDpo = bid - lastma.Sma;

                    decimal? Dpo(CandleList candleList,  int dpoPeriod, int iterator = 0)
                    {
                        int barsback = dpoPeriod / 2 + 1;
                        List<SmaResult> smaForDpo = Serialization.SmaData(candleList, dpoPeriod, deltaPrice);
                        SmaResult lastma = smaForDpo[smaForDpo.Count - barsback - 1 - iterator];
                        if (iterator == 0)
                        {
                            return deltaPrice - lastma.Sma;
                        }
                        else
                        {
                            decimal close = candleList.Candles[candleList.Candles.Count - 1 - iterator].Close;
                            Console.WriteLine("Close: " + close);
                            return close - lastma.Sma; 
                        }

                    }

                    decimal? lastDpo = Dpo(candleList, dpoPeriod);
                    decimal? twoLastDpo = Dpo(candleList, dpoPeriod, 1);
                    decimal? threeLastDpo = Dpo(candleList, dpoPeriod, 2);
                    decimal? fourLastDpo = Dpo(candleList, dpoPeriod, 3);

                    //bool proisvThreeCandle(CandlePayload lastCandle, CandlePayload preLastCandle, CandlePayload prePreLastCandle)
                    //{
                    //    return true;
                    //}

                    decimal? DeltaFourDpo(decimal? lastDpo, decimal? twoLastDpo, decimal? threeLastDpo, decimal? fourPreLastDpo)
                    {
                        var deltaOne = lastDpo - twoLastDpo;
                        var deltaTwo = twoLastDpo - threeLastDpo;
                        var deltaThree = threeLastDpo - fourPreLastDpo;

                        return deltaOne + deltaTwo + deltaThree;
                    }

                    decimal? DeltaAngleFourDpo(decimal? lastDpo, decimal? twoLastDpo, decimal? threeLastDpo, decimal? fourPreLastDpo)
                    {

                        double deltaOne = Convert.ToDouble(lastDpo - twoLastDpo);
                        double angleOne = Math.Atan(deltaOne) * (180 / Math.PI);

                        Console.WriteLine(angleOne);

                        double deltaTwo = Convert.ToDouble(twoLastDpo - threeLastDpo);
                        double angleTwo = Math.Atan(deltaTwo) * (180 / Math.PI);

                        Console.WriteLine(angleTwo);

                        double deltaThree = Convert.ToDouble(threeLastDpo - fourPreLastDpo);
                        double angleThree = Math.Atan(deltaThree) * (180 / Math.PI);

                        Console.WriteLine(angleThree);

                        return Convert.ToDecimal(angleOne + angleTwo + angleThree) /3;
                    }


                    decimal? EmaPriceDelta = 100 - (ema.Last().Ema * 100 / deltaPrice);

                    var obv = Serialization.ObvData(candleList, 10, deltaPrice);
                    var superTrand = Serialization.SuperTrendData(candleList, superTrandPeriod, superTrandSensitive, deltaPrice);

                    var deltaFourDpo = DeltaFourDpo(lastDpo, twoLastDpo, threeLastDpo, fourLastDpo);

                    var deltaAngleFourDpo = DeltaAngleFourDpo(lastDpo, twoLastDpo, threeLastDpo, fourLastDpo);

                    Console.WriteLine();
                    Console.WriteLine("lastDpo: " + lastDpo);
                    Console.WriteLine("deltaAngleFourDpo: " + deltaAngleFourDpo);
                    Console.WriteLine("SmaPriceDelta: " + EmaPriceDelta);
                    Console.WriteLine();

                    Console.WriteLine("twoLastDpo: " + twoLastDpo);
                    Console.WriteLine("threeLastDpo: " + threeLastDpo);
                    Console.WriteLine("fourLastDpo: " + fourLastDpo);

                    Console.WriteLine();

                    Console.WriteLine("Obv: " + obv.Last().Obv);
                    Console.WriteLine("ObvSma: " + obv.Last().ObvSma);
                    Console.WriteLine("deltaPriceOneLast: " + deltaPriceOneLast);
                    Console.WriteLine("deltaPraiceTwoLast: " + deltaPraiceTwoLast);
                    Console.WriteLine();

                    Console.WriteLine("deltaPrice: " + deltaPrice);

                    Console.WriteLine();
                    Console.WriteLine("closePrice: " + closePrice);
                    Console.WriteLine("lastPrise: " + lastPrice);                    
                    Console.WriteLine("bid: " + bid);
                    Console.WriteLine("ask: " + ask);
                    Console.WriteLine();

                    if (superTrand.Last().UpperBand != null)
                    {
                        Console.WriteLine("Go to Sell. UpperBand: " + superTrand.Last().UpperBand);
                    }
                    else 
                    {
                        Console.WriteLine("Go to Buy. LowerBand: " + superTrand.Last().LowerBand);
                    }

                    Console.WriteLine("SuperTrend: " + superTrand.Last().SuperTrend);
                    Console.WriteLine();
                    Console.WriteLine("*****");
                    Console.WriteLine();

                    var portfolio = await context.PortfolioAsync();

                    foreach (var item in portfolio.Positions)
                    {
                        if (item.Figi == figi)
                        {
                            count = item.Lots;
                        }
                    }

                    if (count == 0 
                        && superTrand.Last().UpperBand == null 
                        && lastDpo >= longLastDpoCondition
                        && EmaPriceDelta < EmaPriceDeltaCondition
                        && deltaAngleFourDpo > deltaAngleFourDpoLongCondition)
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Buy, ask));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Buy  price: " + ask + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + EmaPriceDelta + " deltaAngleFourDpo: " + deltaAngleFourDpo);
                        }
                    }
                    else if(count > 0 
                            && (superTrand.Last().LowerBand == null 
                                || lastDpo < fromLongDpoCondition
                                || deltaAngleFourDpo < deltaAngleFourDpoFromLongCondition))
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Sell, bid));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Sell  price: " + bid + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + EmaPriceDelta + " deltaAngleFourDpo: " + deltaAngleFourDpo);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                finally 
                {
                }

                Thread.Sleep(250);
            }
                
        }
    }
}
