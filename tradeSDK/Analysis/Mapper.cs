using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;
using DataCollector.Models;

namespace TinkoffData
{
    public static class Mapper 
    {
        public static List<Quote> ConvertTinkoffCandlesToQuote(List<CandlePayload> candles)
        {
            List<Quote> quotes = new List<Quote>();

            foreach (var candle in candles)
            {
                Quote quote = new Quote();
                quote.Close = candle.Close;
                quote.Date = candle.Time;
                quote.Open = candle.Open;
                quote.High = candle.High;
                quote.Low = candle.Low;
                quote.Volume = candle.Volume;
                quotes.Add(quote);
            }
            return quotes;
        }


        public static List<Quote> ConvertTinkoffCandlesToQuote(List<CandlePayload> candles, decimal realClose)
        {
            List<Quote> quotes = new List<Quote>();

            foreach (var candle in candles)
            {
                Quote quote = new Quote();
                quote.Close = candle.Close;
                quote.Date = candle.Time;
                quote.Open = candle.Open;
                quote.High = candle.High;
                quote.Low = candle.Low;
                quote.Volume = candle.Volume;
                quotes.Add(quote);
            }
            quotes.Last().Close = realClose;
            return quotes;
        }


        public static List<AdxResult> TsiData(CandleList candleList, int lookbackPeriod)
        {
            throw new NotImplementedException();
        }
        public static List<AdxResult> TsiData(CandleList candleList, decimal deltaPrice, int lookbackPeriod)
        {
            throw new NotImplementedException();
        }


        public static List<AdxResult> AdxData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<AdxResult> adx = Indicator.GetAdx(candles, lookbackPeriod).ToList();
            return adx;
        }

        public static List<AdxResult> AdxData(CandleList candleList, decimal realPrise, int lookbackPeriod)
        {
            try
            {
                Log.Information("Start AdxData method. Figi: " + candleList.Figi);
                Log.Information("Candles count: " + candleList.Candles.Count);
                Log.Information("realPrise: " + realPrise);
                Log.Information("lookbackPeriod: " + lookbackPeriod);
                List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
                List<AdxResult> adx = Indicator.GetAdx(candles, lookbackPeriod).ToList();
                Log.Information("Stop AdxData method. Figi: " + candleList.Figi);
                return adx;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Information("Stop AdxData method. Return: null");
                return null;
            }
        }

        public static List<AroonResult> AroonData(CandleList candleList, int lookbackPeriod = 7)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<AroonResult> aroon = Indicator.GetAroon(candles, lookbackPeriod).ToList();
            return aroon;
        }

        public static List<AroonResult> AroonData(CandleList candleList, decimal realPrise, int lookbackPeriod = 7)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List<AroonResult> aroon = Indicator.GetAroon(candles, lookbackPeriod).ToList();
            return aroon;
        }

        public static List<BollingerBandsResult> BollingerBandsData(CandleList candleList, decimal realPrise, int lookbackPeriod = 20, decimal standardDeviations = 2m)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List<BollingerBandsResult> bollingerBands = Indicator.GetBollingerBands(candles, lookbackPeriod, standardDeviations).ToList();
            return bollingerBands;
        }
        public static List<BollingerBandsResult> BollingerBandsData(CandleList candleList, int lookbackPeriod = 20, decimal standardDeviations = 2m)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<BollingerBandsResult> bollingerBands = Indicator.GetBollingerBands(candles, lookbackPeriod, standardDeviations).ToList();
            return bollingerBands;
        }

        public static List<MacdResult> MacdData(CandleList candleList)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<MacdResult> macdResult = Indicator.GetMacd(candles).ToList();
            return macdResult;
        }

        //public static List<MacdResult> MacdData(CandleList candleList, decimal realPrise, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        public static List<MacdResult> MacdData(CandleList candleList, decimal realPrise, int fastPeriod, int slowPeriod, int signalPeriod)
        {
            Log.Information("Start MacdData method:");
            Log.Information("Figi: " + candleList.Figi);
            Log.Information("Candles count: " + candleList.Candles.Count);
            Log.Information("fastPeriod: " + fastPeriod);
            Log.Information("slowPeriod: " + slowPeriod);
            Log.Information("signalPeriod: " + signalPeriod);
            List <Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List <MacdResult> macdResult = Indicator.GetMacd(candles, fastPeriod, slowPeriod, signalPeriod).ToList();
            Log.Information("Stop MacdData method:");
            return macdResult;
        }

        public static List<EmaResult> EmaData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetEma(candles, lookbackPeriod).ToList();
        }

        public static List<EmaResult> EmaData(CandleList candleList, int lookbackPeriod, decimal realPrise)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetEma(candles, lookbackPeriod).ToList();
        }

        public static List<ObvResult> ObvData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetObv(candles, lookbackPeriod).ToList();
        }
        
        public static List<ObvResult> ObvData(CandleList candleList, decimal realPrise, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetObv(candles, lookbackPeriod).ToList();
        }

        public static List<AdlResult> AdlData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetAdl(candles, lookbackPeriod).ToList();
        }

        public static List<AdlResult> AdlData(CandleList candleList, decimal realPrise, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetAdl(candles, lookbackPeriod).ToList();
        }

        public static List<SmaResult> SmaData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetSma(candles, lookbackPeriod).ToList();
        }
        public static List<SmaResult> SmaData(CandleList candleList, decimal realPrise, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetSma(candles, lookbackPeriod).ToList();
        }

        public static List<DpoResult> DpoData(CandleList candleList, int lookbackPeriod)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetDpo(candles, lookbackPeriod).ToList();
        }

        public static List<DpoResult> DpoData(CandleList candleList, decimal realPrise, int lookbackPeriod)
        {
            Log.Information("DPO set price = " + realPrise);
            Log.Information("DPO set lookbackPeriod = " + lookbackPeriod);
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List <DpoResult> dpoData = Indicator.GetDpo(candles, lookbackPeriod).ToList();
            Log.Information("Last Dpo = " + dpoData.Last().Dpo + " " + dpoData.Last().Date);
            return dpoData;
        }

        public static List<SuperTrendResult> SuperTrendData(CandleList candleList, int lookbackPeriod = 20, decimal multiplier = 2)
        {
            Log.Information("Super Trend History = " + lookbackPeriod);
            Log.Information("Super Trend Multiplier = " + multiplier);

            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<SuperTrendResult> superTrandData = Indicator.GetSuperTrend(candles, lookbackPeriod, multiplier).ToList();

            Log.Information("Super Trend Value = " + superTrandData.Last().SuperTrend + " " + superTrandData.Last().Date);

            return superTrandData;
        }

        public static List<SuperTrendResult> SuperTrendData(CandleList candleList, decimal realPrise, int lookbackPeriod = 20, decimal multiplier = 2)
        {

            Log.Information("Average (Bid, Ask) Prise = " + realPrise);
            Log.Information("Super Trend History = " + lookbackPeriod);
            Log.Information("Super Trend Multiplier = " + multiplier);

            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List<SuperTrendResult> superTrandData = Indicator.GetSuperTrend(candles, lookbackPeriod, multiplier).ToList();

            Log.Information("Super Trend Value = " + superTrandData.Last().SuperTrend + " " + superTrandData.Last().Date);

            return superTrandData;
        }

        public static List<IchimokuResult> IchimokudData(CandleList candleList, int signalPeriod = 9, int shortSpanPeriod = 26, int longSpanPeriod = 52)
        {

            Log.Information("signalPeriod = " + signalPeriod);
            Log.Information("shortSpanPeriod = " + shortSpanPeriod);
            Log.Information("longSpanPeriod = " + longSpanPeriod);

            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            List<IchimokuResult> ichimokuData = Indicator.GetIchimoku(candles, signalPeriod, shortSpanPeriod, longSpanPeriod).ToList();

            Log.Information("TenkanSen = " + ichimokuData.Last().TenkanSen + " " + ichimokuData.Last().Date);
            Log.Information("KijunSen = " + ichimokuData.Last().KijunSen + " " + ichimokuData.Last().Date);
            Log.Information("SenkouSpanA = " + ichimokuData.Last().SenkouSpanA + " " + ichimokuData.Last().Date);
            Log.Information("SenkouSpanB = " + ichimokuData.Last().SenkouSpanB + " " + ichimokuData.Last().Date);
            Log.Information("ChikouSpan = " + ichimokuData.Last().ChikouSpan + " " + ichimokuData.Last().Date);

            return ichimokuData;
        }

        public static List<IchimokuResult> IchimokuData(CandleList candleList, decimal realPrise, int signalPeriod = 9, int shortSpanPeriod = 26, int longSpanPeriod = 52)
        {

            Log.Information("realPrise = " + realPrise);
            Log.Information("signalPeriod = " + signalPeriod);
            Log.Information("shortSpanPeriod = " + shortSpanPeriod);
            Log.Information("longSpanPeriod = " + longSpanPeriod);

            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            List<IchimokuResult> ichimokuData = Indicator.GetIchimoku(candles, signalPeriod, shortSpanPeriod, longSpanPeriod).ToList();

            Log.Information("TenkanSen = " + ichimokuData.Last().TenkanSen + " " + ichimokuData.Last().Date);
            Log.Information("KijunSen = " + ichimokuData.Last().KijunSen + " " + ichimokuData.Last().Date);
            Log.Information("SenkouSpanA = " + ichimokuData.Last().SenkouSpanA + " " + ichimokuData.Last().Date);
            Log.Information("SenkouSpanB = " + ichimokuData.Last().SenkouSpanB + " " + ichimokuData.Last().Date);
            Log.Information("ChikouSpan = " + ichimokuData.Last().ChikouSpan + " " + ichimokuData.Last().Date);

            return ichimokuData;
        }
    }
}
