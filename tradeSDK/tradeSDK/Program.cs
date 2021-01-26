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
using System.Diagnostics;
using Serilog;


namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
                
                .CreateLogger();        
                        Market market = new Market();
            SandboxContext context = new Auth().GetSanboxContext();
            //Serialization ser = new Serialization();

            var figi = "BBG000BVPV84";
            var candleInterval = CandleInterval.Minute;
            int CandleCount = 110;

            //System config
            int sleep = 0;

            //DPO config 
            int dpoPeriod = 20;
            int dpoAverageAngleCountLong = 3;
            double dpoAverageAngleConditionLong = 30;
            int dpoAverageAngleCountFromLong = 4;
            double dpoAverageAngleConditionFromLong = -20;

            decimal longLastDpoCondition = 0;
            decimal fromLongDpoCondition = 0;

            //Ema config
            //int emaPeriod = 10;
            decimal ichimokuTenkansenPriceDeltaCount = 0.12M;

            //Super Trend config
            int superTrandPeriod = 20;
            int superTrandSensitive = 2;

            //Ichimoku
            int ichimokuDeltaAngleCountLong = 3;
            double ichimokuTenkanSenAngleLong = 20;

            while (true)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                try
                {
                    int count = 0;
                    Log.Information("Start cicle");
                    CandleList candleList = await market.GetCandlesTinkoff(context, figi, candleInterval, CandleCount);
                    Orderbook orderbook = await context.MarketOrderbookAsync(figi, 1);
                    if (orderbook.Asks.Count == 0)
                    {
                        Log.Information("Биржа по инструменту " + figi + " не работает");
                        Thread.Sleep(sleep);
                        continue;
                    }

                    var ask = orderbook.Asks.Last().Price;
                    var bid = orderbook.Bids.Last().Price;

                    var lastPrice = orderbook.LastPrice;
                    var closePrice = orderbook.ClosePrice;

                    var deltaPrice = (ask + bid) / 2;

                    List<SmaResult> smaForDpo = Serialization.SmaData(candleList, dpoPeriod, deltaPrice);
                    //List<EmaResult> ema = Serialization.EmaData(candleList, emaPeriod, deltaPrice);
                    List<DpoResult> dpo = Serialization.DpoData(candleList, deltaPrice, dpoPeriod);
                    List<SuperTrendResult> superTrand = Serialization.SuperTrendData(candleList, superTrandPeriod, superTrandSensitive, deltaPrice);
                    List<IchimokuResult> ichimoku = Serialization.IchimokuData(candleList, deltaPrice);

                    bool ichimokuLongLine(List<IchimokuResult> ichimoku, decimal? price)
                    {
                        if (ichimoku.Last().TenkanSen >= ichimoku.Last().KijunSen
                            && price > ichimoku.Last().SenkouSpanA
                            && price > ichimoku.Last().SenkouSpanB
                            && price > ichimoku.Last().TenkanSen
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanA
                            && ichimoku.Last().TenkanSen > ichimoku.Last().SenkouSpanB)
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
                            Log.Information("Tenkansen: " + item.Date + " " + item.TenkanSen);
                        }

                        return DeltaDegreeAngle(values);
                    }

                    double DpoDegreeAverageAngle(List<DpoResult> dpo, int anglesCount)
                    {
                        List<DpoResult> skipDpo = dpo.Skip(dpo.Count - (anglesCount + 1)).ToList();
                        List<decimal?> values = new List<decimal?>();
                        foreach (var item in skipDpo)
                        {
                            values.Add(item.Dpo);
                            Log.Information("DPO: " + item.Date + " " + item.Dpo);
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
                            Log.Information("Angle: " + angle.ToString());
                            summ += angle;
                        }
                        double averageAngles = summ / (countDelta - 1);
                        Log.Information("Average Angles: " + averageAngles.ToString());
                        return averageAngles;
                    }

                    // decimal? emaPriceDelta = 100 - (ema.Last().Ema * 100 / deltaPrice); //Насколько далеко убежала цена от Ema

                    var portfolio = await context.PortfolioAsync();


                    decimal? ichimokuTenkansenPriceDelta = 100 - (ichimoku.Last().TenkanSen * 100 / deltaPrice); //Насколько далеко убежала цена от Ichimoku TenkanSen


                    foreach (var item in portfolio.Positions)
                    {
                        if (item.Figi == figi)
                        {
                            count = item.Lots;
                        }
                    }
                    //Console.WriteLine(stopWatch.ElapsedMilliseconds);
                    if (count == 0
                        && superTrand.Last().UpperBand == null
                        && lastDpo >= longLastDpoCondition
                        && ichimokuTenkansenPriceDelta < ichimokuTenkansenPriceDeltaCount
                        && DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong) > dpoAverageAngleConditionLong
                        && ichimokuLongLine(ichimoku, deltaPrice)
                        && ichmokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong) > ichimokuTenkanSenAngleLong
                        )
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Buy, ask));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Buy  price: " + ask + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + ichimokuTenkansenPriceDelta + " DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                            sw.WriteLine(@" Ichimoku: " + ichimoku.Last().TenkanSen + " " + ichimoku.Last().KijunSen + " " + ichimoku.Last().SenkouSpanA + " " + ichimoku.Last().SenkouSpanB);

                            sw.WriteLine();
                        }
                    }
                    else if (count > 0
                            && (
                            (lastDpo < fromLongDpoCondition && superTrand.Last().LowerBand == null)
                                || (DpoDegreeAverageAngle(dpo, dpoAverageAngleCountFromLong) < dpoAverageAngleConditionFromLong) && superTrand.Last().LowerBand == null))
                    {
                        await context.PlaceLimitOrderAsync(new LimitOrder(figi, 1, OperationType.Sell, bid));
                        using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Sell  price: " + bid + " UpperBand: " + superTrand.Last().UpperBand + " LowerBand: " + superTrand.Last().LowerBand + " DPO: " + lastDpo + " SmaPriceDelta: " + ichimokuTenkansenPriceDelta + " DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                            sw.WriteLine(@" Ichimoku: " + ichimoku.Last().TenkanSen + " " + ichimoku.Last().KijunSen + " " + ichimoku.Last().SenkouSpanA + " " + ichimoku.Last().SenkouSpanB);

                            sw.WriteLine();
                        }
                    }
                    Log.Information("Price: " + deltaPrice);

                    if (superTrand.Last().UpperBand != null)
                    {
                        Log.Information("SuperTrand: Sell");
                    }
                    else
                    {
                        Log.Information("SuperTrand: Buy");
                    }
                    Log.Information("lastDpo: " + lastDpo);
                    Log.Information("IchimokuTenkansenPriceDelta: " + ichimokuTenkansenPriceDelta);


                    Log.Information("DpoDegreeAverageAngle: " + DpoDegreeAverageAngle(dpo, dpoAverageAngleCountLong));
                    Log.Information("ichimoku: ");
                    Log.Information("     TenkanSen: " + ichimoku.Last().TenkanSen);
                    Log.Information("     KijunSen: " + ichimoku.Last().KijunSen);
                    Log.Information("     SenkouSpanA: " + ichimoku.Last().SenkouSpanA);
                    Log.Information("     SenkouSpanB: " + ichimoku.Last().SenkouSpanB);
                    Log.Information("     SenkouSpanB: " + ichimoku.Last().SenkouSpanB);
                    Log.Information("     ichmokuTenkansenAngleLong: " + ichmokuTenkansenDegreeAverageAngle(ichimoku, ichimokuDeltaAngleCountLong));
                }

                catch (Exception ex)
                {
                    Log.Information(ex.ToString());
                    sleep += 10;
                }
                finally
                {
                }

                sleep = DynamicSleep(sleep);

                stopWatch.Stop();
                Log.Information(stopWatch.ElapsedMilliseconds.ToString());
                Log.Information("Stop cicle");

                Log.Information("Sleep: " + sleep);
                Thread.Sleep(sleep);
            }
        }

        private static int DynamicSleep(int sleep)
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
