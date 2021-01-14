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
            int superTrandPeriod = 10;
            int superTrandSensitive = 2;
            decimal SmaPriceDeltaCondition = 0.1M;
            decimal DeltaThreeDpoLongCondition = 1;
            decimal DeltaThreeDpoFromLongCondition = -1;
            while (true)
            {
                try
                {               

                    int count = 0;
                    var figi = "BBG000BVPV84";
                    var date = DateTime.Now;
                    var candleInterval = CandleInterval.FiveMinutes;

                    CandleList candleList = await market.GetCandleByFigi(context, figi, candleInterval, date);

                    //CandleList first = await market.GetCandleByFigi(context, figi, candleInterval, date);
                    //CandleList second = await market.GetCandleByFigi(context, figi, candleInterval, date.AddDays(-1));

                    //List<CandlePayload> allCandles = first.Candles.Union(second.Candles).ToList();
                    //CandleList candleList = new CandleList(figi, candleInterval, allCandles);

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
                    List<SmaResult> sma = Serialization.SmaData(candleList, dpoPeriod, bid);
                    //SmaResult lastma = sma[sma.Count - barsback -1];
                    //decimal? lastDpo = bid - lastma.Sma;

                    decimal? Dpo(CandleList candleList,  int dpoPeriod, int iterator = 0)
                    {
                        int barsback = dpoPeriod / 2 + 1;
                        List<SmaResult> sma = Serialization.SmaData(candleList, dpoPeriod, bid);
                        SmaResult lastma = sma[sma.Count - barsback - 1 - iterator];
                        if (iterator == 0)
                        {
                            return bid - lastma.Sma;
                        }
                        else
                        {
                            decimal close = candleList.Candles[candleList.Candles.Count - 1 - iterator].Close;
                            return close - lastma.Sma; 
                        }

                    }

                    decimal? lastDpo = Dpo(candleList, dpoPeriod);
                    decimal? twoLastDpo = Dpo(candleList, dpoPeriod, 1);
                    decimal? threeLastDpo = Dpo(candleList, dpoPeriod, 2);
                    decimal? fourLastDpo = Dpo(candleList, dpoPeriod, 2);

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


                    decimal? SmaPriceDelta = 100 - (sma.Last().Sma * 100 / bid);


                    var obv = Serialization.ObvData(candleList, 10, bid);
                    var superTrand = Serialization.SuperTrendData(candleList, superTrandPeriod, superTrandSensitive, bid);


                    var deltaFourDpo = DeltaFourDpo(lastDpo, twoLastDpo, threeLastDpo, fourLastDpo);


                    Console.WriteLine("lastDpo: " + lastDpo);
                    Console.WriteLine("twoLastDpo: " + twoLastDpo);
                    Console.WriteLine("threeLastDpo: " + threeLastDpo);
                    Console.WriteLine("fourLastDpo: " + fourLastDpo);

                    Console.WriteLine();
                    Console.WriteLine("deltaFourDpo: " + deltaFourDpo);
                    Console.WriteLine("SmaPriceDelta: " + SmaPriceDelta);
                    Console.WriteLine();

                    Console.WriteLine("Obv: " + obv.Last().Obv);
                    Console.WriteLine("ObvSma: " + obv.Last().ObvSma);
                    Console.WriteLine("deltaPriceOneLast: " + deltaPriceOneLast);
                    Console.WriteLine("deltaPraiceTwoLast: " + deltaPraiceTwoLast);
                    Console.WriteLine("deltaPrice: " + deltaPrice);
                    Console.WriteLine("closePrice: " + closePrice);
                    Console.WriteLine("lastPrise: " + lastPrice);                    
                    Console.WriteLine("bid: " + bid);
                    Console.WriteLine("ask: " + ask);
                    Console.WriteLine("SmaPriceDelta: " + SmaPriceDelta);
                    Console.WriteLine("LowerBand: " + superTrand.Last().LowerBand);
                    Console.WriteLine("UpperBand: " + superTrand.Last().UpperBand);
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
                        && lastDpo >= 0 
                        && SmaPriceDelta < SmaPriceDeltaCondition
                        && deltaFourDpo > DeltaThreeDpoLongCondition)
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Buy, ask));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Buy  price: " + ask + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + SmaPriceDelta);
                        }
                    }
                    else if(count > 0 
                            && (superTrand.Last().LowerBand == null 
                                || lastDpo < 0
                                || deltaFourDpo < DeltaThreeDpoFromLongCondition))
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Sell, bid));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Sell  price: " + bid + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + SmaPriceDelta);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally { }
                Thread.Sleep(300);
            }
                
        }
    }
}
