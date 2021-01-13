using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
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

            int count = 0;
            var figi = "BBG000BVPV84";
            var candleList = await market.GetCandleByFigi(context, figi, CandleInterval.FiveMinutes, DateTime.Now);
            Console.WriteLine(candleList.Figi);
            //static List<Quote> ConvertTinkoffCandlesToQuote(List<CandlePayload> candles, decimal realClose)
            //{
            //    List<Quote> quotes = new List<Quote>();

            //    foreach (var candle in candles)
            //    {
            //        Quote quote = new Quote();
            //        quote.Close = candle.Close;
            //        quote.Date = candle.Time;
            //        quote.Open = candle.Open;
            //        quote.High = candle.High;
            //        quote.Low = candle.Low;
            //        quote.Volume = candle.Volume;
            //        quotes.Add(quote);
            //    }
            //    quotes.Last().Close = realClose;
            //    return quotes;
            //}


            var ema = Serialization.EmaData(candleList, 20);

            Console.WriteLine(ema.Last().Date);
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
