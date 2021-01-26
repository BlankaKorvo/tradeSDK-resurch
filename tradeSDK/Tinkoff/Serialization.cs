using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace Tinkoff
{
    public static class Serialization
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

        public static List<EmaResult> EmaData(CandleList candleList, int history)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetEma(candles, history).ToList();
        }

        public static List<EmaResult> EmaData(CandleList candleList, int history, decimal realPrise)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetEma(candles, history).ToList();
        }

        public static List<ObvResult> ObvData(CandleList candleList, int history)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetObv(candles, history).ToList();
        }
        
        public static List<ObvResult> ObvData(CandleList candleList, int history, decimal realPrise)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetObv(candles, history).ToList();
        }

        public static List<SmaResult> SmaData(CandleList candleList, int history)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetSma(candles, history).ToList();
        }
        public static List<SmaResult> SmaData(CandleList candleList, int history, decimal realPrise)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetSma(candles, history).ToList();
        }

        public static List<DpoResult> DpoData(CandleList candleList, int history = 20)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetDpo(candles, history).ToList();
        }
        public static List<DpoResult> DpoData(CandleList candleList, decimal realPrise, int history = 20)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetDpo(candles, history).ToList();
        }

        public static List<SuperTrendResult> SuperTrendData(CandleList candleList, int history = 20, decimal multiplier = 2)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetSuperTrend(candles, history, multiplier).ToList();
        }

        public static List<SuperTrendResult> SuperTrendData(CandleList candleList, decimal realPrise, int history = 20, decimal multiplier = 2)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetSuperTrend(candles, history, multiplier).ToList();
        }

        public static List<IchimokuResult> IchimokudData(CandleList candleList, int signalPeriod = 9, int shortSpanPeriod = 26, int longSpanPeriod = 52)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetIchimoku(candles, signalPeriod, shortSpanPeriod, longSpanPeriod).ToList();
        }

        public static List<IchimokuResult> IchimokuData(CandleList candleList, decimal realPrise, int signalPeriod = 9, int shortSpanPeriod = 26, int longSpanPeriod = 52)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles, realPrise);
            return Indicator.GetIchimoku(candles, signalPeriod, shortSpanPeriod, longSpanPeriod).ToList();
        }
    }
}
