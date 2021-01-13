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
        public static List<EmaResult> EmaData(CandleList candleList, int history)
        {
            List<Quote> candles = Serialization.ConvertTinkoffCandlesToQuote(candleList.Candles);
            return Indicator.GetEma(candles, history).ToList();
        }

    }
}
