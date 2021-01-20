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

            var figi = "BBG000BVPV84";
            var candleInterval = CandleInterval.Minute;
            int CandleCount = 110;

            //System config
            int sleep = DynamicSleep(0);

            //DPO config
            int dpoPeriod = 20;
            int dpoAverageAngleCountLong = 2;
            double dpoAverageAngleConditionLong = 20;
            int dpoAverageAngleCountFromLong = 4;
            double dpoAverageAngleConditionFromLong = -5;

            decimal longLastDpoCondition = 0;
            decimal fromLongDpoCondition = 0;

            //Ema config
            int emaPeriod = 10;
            decimal EmaPriceDeltaCondition = 0.12M;

            //Super Trend config
            int superTrandPeriod = 20;
            int superTrandSensitive = 2;

            //Ichimoku
            int ichimokuDeltaAngleCountLong = 2;
            double ichimokuTenkanSenAngleLong = 10;

            while (true)
            {
                try
                {
                    int count = 0;

                    CandleList candleList = await market.GetCandlesTinkoff(context, figi, candleInterval, CandleCount);

                    Orderbook orderbook = await context.MarketOrderbookAsync(figi, 1);
                    if (orderbook.Asks.Count == 0)
                    {
                        Console.WriteLine(DateTime.Now + "  биржа не фурычит");
                        Thread.Sleep(sleep);
                        continue;
                    }

                    var ask = orderbook.Asks.Last().Price;
                    var bid = orderbook.Bids.Last().Price;

                    var lastPrice = orderbook.LastPrice;
                    var closePrice = orderbook.ClosePrice;

                    var deltaPrice = (ask + bid) / 2;

                    List<SmaResult> smaForDpo = Serialization.SmaData(candleList, dpoPeriod, deltaPrice);
                    List<EmaResult> ema = Serialization.EmaData(candleList, emaPeriod, deltaPrice);
                    List<DpoResult> dpo = Serialization.DpoData(candleList, dpoPeriod, deltaPrice);
                    List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, superTrandPeriod, superTrandSensitive, deltaPrice);
                    List<IchimokuResult> ichimoku = Serialization.IchimokuData(candleList, deltaPrice);

                    bool ichimokuLongLine(List<IchimokuResult> ichimoku, decimal? price)
                    {
                        if (ichimoku.Last().TenkanSen > ichimoku.Last().KijunSen
                            && price > ichimoku.Last().SenkouSpanA
                            && price > ichimoku.Last().SenkouSpanB
                            && price > ichimoku.Last().TenkanSen)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    double ichmokuTenkansenDegreeAverageAngle(List<IchimokuResult> ichimoku, int anglesCount)
                    {
                        List<IchimokuResult> skipIchimoku = ichimoku.Skip(ichimoku.Count - (anglesCount + 1)).ToList();
                        List<decimal?> values = new List<decimal?>();
                        foreach (var item in skipIchimoku)
                        {
                            values.Add(item.TenkanSen);
                        }

                        return DeltaDegreeAngle(values);
                    }

                    double DpoDegreeAverageAngle(List<DpoResult> dpo, int anglesCount)
                    {
                        List<DpoResult> skipDpo = dpo.Skip(dpo.Count - (anglesCount + 1)).ToList();
                        List<decimal?> values = new List<decimal?>();
                        foreach (var item in skipDpo)
                        {
                            Console.WriteLine(item.Dpo);
                            values.Add(item.Dpo);
                        }

                        return DeltaDegreeAngle(values);
                    }

                    decimal? lastDpo = dpo.Last().Dpo;
                   

                    double DeltaDegreeAngle(List<decimal?> values)
                    {
                        var countDelta = values.Count;
                        double summ = 0;
                        for (int i = 1; i < countDelta; i++)
                        {
                            double deltaLeg = Convert.ToDouble(values[i] - values[i - 1]);
                            double legDifference = Math.Atan(deltaLeg);
                            double angle = legDifference * (180 / Math.PI);
                            summ += angle;
                        }
                        return summ / (countDelta - 1);
                    }

                    decimal? IchimokuTenkansenPriceDelta = 100 - (ema.Last().Ema * 100 / deltaPrice); //Насколько далеко убежала цена от Ema

                    var portfolio = await context.PortfolioAsync();


                    decimal? ichimokuTenkansenPriceDelta = 100 - (ichimoku.Last().TenkanSen * 100 / deltaPrice); //Насколько далеко убежала цена от Ichimoku TenkanSen


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
                        && IchimokuTenkansenPriceDelta < ichimokuTenkansenPriceDelta
                        && DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong) > dpoAverageAngleConditionLong
                        && ichimokuLongLine(ichimoku, deltaPrice)
                        && ichmokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong) > ichimokuTenkanSenAngleLong
                        )
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Buy, ask));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Buy  price: " + ask + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + IchimokuTenkansenPriceDelta + " DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                            sw.WriteLine(@" Ichimoku: " + ichimoku.Last().TenkanSen + ichimoku.Last().KijunSen + ichimoku.Last().SenkouSpanA + ichimoku.Last().SenkouSpanB);

                            sw.WriteLine();
                        }
                    }
                    else if (count > 0
                            && (superTrand.Last().LowerBand == null
                                || lastDpo < fromLongDpoCondition
                                || DpoDegreeAverageAngle(dpo, dpoAverageAngleCountFromLong) > dpoAverageAngleConditionFromLong))
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Sell, bid));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Sell  price: " + bid + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + IchimokuTenkansenPriceDelta + " DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                            sw.WriteLine(@" Ichimoku: " + ichimoku.Last().TenkanSen + ichimoku.Last().KijunSen + ichimoku.Last().SenkouSpanA + ichimoku.Last().SenkouSpanB);

                            sw.WriteLine();
                        }
                    }

                    Console.WriteLine("********");
                    Console.WriteLine("Long condition:");
                    Console.WriteLine();
                    Console.WriteLine("Price: " + deltaPrice);
                    Console.WriteLine();
                    if (superTrand.Last().UpperBand != null)
                    {
                        Console.WriteLine("Go to Sell");
                    }
                    else
                    {
                        Console.WriteLine("Go to Buy");
                    }
                    Console.WriteLine();
                    Console.WriteLine("lastDpo: " + lastDpo);
                    Console.WriteLine();
                    Console.WriteLine("IchimokuTenkansenPriceDelta: " + IchimokuTenkansenPriceDelta);
                    Console.WriteLine();
                    Console.WriteLine("DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                    Console.WriteLine("ichimoku: ");
                    Console.WriteLine("     TenkanSen: " + ichimoku.Last().TenkanSen);
                    Console.WriteLine("     KijunSen: " + ichimoku.Last().KijunSen);
                    Console.WriteLine("     SenkouSpanA: " + ichimoku.Last().SenkouSpanA);
                    Console.WriteLine("     SenkouSpanB: " + ichimoku.Last().SenkouSpanB);
                    Console.WriteLine("     SenkouSpanB: " + ichimoku.Last().SenkouSpanB);
                    Console.WriteLine("     ichmokuTenkansenAngleLong: " + ichmokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong));

                    //Console.WriteLine();
                    //Console.WriteLine("lastDpo: " + lastDpo);
                    //Console.WriteLine("deltaAngleFourDpo: " + deltaAngleFourDpo);
                    //Console.WriteLine("SmaPriceDelta: " + EmaPriceDelta);
                    //Console.WriteLine();
                    

                    //Console.WriteLine("lastDpo: " + lastDpo + dpo.Last().Dpo);
                    //Console.WriteLine("twoLastDpo: " + twoLastDpo + " "+ dpo[dpo.Count - 2].Dpo);
                    //Console.WriteLine("threeLastDpo: " + threeLastDpo + " " + dpo[dpo.Count - 3].Dpo);
                    //Console.WriteLine("fourLastDpo: " + fourLastDpo + " " + dpo[dpo.Count - 4].Dpo);

                    //Console.WriteLine();
                    //Console.WriteLine("Ichimoku: " + ichimoku.Last().TenkanSen + " " + ichimoku.Last().KijunSen + " " + ichimoku.Last().SenkouSpanA + " " + ichimoku.Last().SenkouSpanB);

                    //Console.WriteLine("Obv: " + obv.Last().Obv);
                    //Console.WriteLine("ObvSma: " + obv.Last().ObvSma);
                    //Console.WriteLine("deltaPriceOneLast: " + deltaPriceOneLast);
                    //Console.WriteLine("deltaPraiceTwoLast: " + deltaPraiceTwoLast);
                    //Console.WriteLine();

                    //Console.WriteLine("deltaPrice: " + deltaPrice);

                    //Console.WriteLine();
                    //Console.WriteLine("closePrice: " + closePrice);
                    //Console.WriteLine("lastPrise: " + lastPrice);
                    //Console.WriteLine("bid: " + bid);
                    //Console.WriteLine("ask: " + ask);
                    //Console.WriteLine();

                    //if (superTrand.Last().UpperBand != null)
                    //{
                    //    Console.WriteLine("Go to Sell. UpperBand: " + superTrand.Last().UpperBand);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Go to Buy. LowerBand: " + superTrand.Last().LowerBand);
                    //}

                    //Console.WriteLine("SuperTrend: " + superTrand.Last().SuperTrend);
                    //Console.WriteLine();
                    //Console.WriteLine("*****");
                    //Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sleep += 10;
                }
                finally
                {
                }               

                Console.WriteLine("Sleep!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: " + sleep);
                Thread.Sleep(sleep);
            }
        }

        static int DynamicSleep(int sleep)
        {
            sleep -= 1;
            if (sleep < 0)
            {
                sleep = 0;
            }
            else if (sleep > 1000)
            {
                sleep = 1000;
            }
            return sleep;
        }
    }
}
