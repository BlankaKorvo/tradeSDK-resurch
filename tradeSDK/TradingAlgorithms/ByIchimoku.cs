using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TradingAlgorithms
{
    public class ByIchimoku
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

        public List<IchimokuResult> IchimokuDate(CandleList candleList)
        {
            List<Quote> candles = ConvertTinkoffCandlesToQuote(candleList.Candles); //Нужно написать новые сериализвтор из тинькова в скендер. Сверху два метода - это костыли, которые скорее всего не работают.
            return Indicator.GetIchimoku(candles).ToList();
        }

        public async Task OperationByClassicalSignal(CandleList candleLists, int lots, Context context)
        {
            List<IchimokuResult> ichimokuResult = IchimokuDate(candleLists);
            IchimokuResult currentIchResult = ichimokuResult.Last();
            IchimokuResult preCurrentIchResult = ichimokuResult[ichimokuResult.Count - 2];

            CandlePayload currentCandle = candleLists.Candles.Last();
            CandlePayload preCurrentCandle = candleLists.Candles[candleLists.Candles.Count - 2]; /// Ошибка?? [candleLists.Candles.Count - 2]


            Portfolio portfolio = await context.PortfolioAsync();
            string figi = candleLists.Candles.Last().Figi;
            var instrument = await context.MarketSearchByFigiAsync(figi);
            string nameLot = instrument.Name;
            int lotsFact = 0;
            decimal balance = 0M;
            var stakan = await context.MarketOrderbookAsync(figi, 1);
            var bid = stakan.Bids.Last().Price;
            var ask = stakan.Asks.Last().Price;
            //decimal closePrice = stakan.LastPrice;
            //decimal closePrice = stakan.Asks.FirstOrDefault().Price;
            if (stakan.Asks.Count == 0)
            {
                Console.WriteLine("Биржа не работает");
                // return;
            }
            else
            {
                //decimal closePrice = stakan.Asks.FirstOrDefault().Price;
                decimal closePrice = bid;
                foreach (var item in portfolio.Positions)
                {
                    if (item.Figi == figi)
                    {
                        lotsFact = item.Lots;
                        balance = item.Balance;
                        break;
                    }
                }

                //Long
                if (LongSignal(lots, currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, lotsFact, closePrice))
                {
                    int quantity = lots - lotsFact;
                    //await context.PlaceMarketOrderAsync(new MarketOrder(figi, quantity, OperationType.Buy));
                    await context.PlaceLimitOrderAsync(new LimitOrder(figi, quantity, OperationType.Buy, ask));  // only in SANDBOX!!!!!!!!!!!!!!!!!!1

                    ConsoleOutInformation(preCurrentIchResult, currentCandle, figi, nameLot, balance, closePrice);

                    using (StreamWriter sw = new StreamWriter("buy", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" Buy " + nameLot + " quantity: " + quantity + " price: " + closePrice);
                    }

                }

                //Go to cash, after long
                else if (EndLongSignal(currentIchResult, preCurrentIchResult, lotsFact, closePrice))
                {
                    //await context.PlaceMarketOrderAsync(new MarketOrder(figi, lotsFact, OperationType.Sell));
                    await context.PlaceLimitOrderAsync(new LimitOrder(figi, lotsFact, OperationType.Sell, bid)); // only in SANDBOX!!!!!!!!!!!!!!!!!!1

                    Console.WriteLine(@"Sell " + nameLot + "quantity: " + lotsFact + " price: " + closePrice);
                    int quantity = lotsFact;

                    ConsoleOutInformation(preCurrentIchResult, currentCandle, figi, nameLot, balance, closePrice);

                    using (StreamWriter sw = new StreamWriter("gotocach", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" Sell " + nameLot + "quantity: " + quantity + " price: " + closePrice);
                    }
                }

                //Short
                else if (ShortSignal(lots, currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, lotsFact, closePrice))
                {
                    int quantity = lotsFact + lots;

                    await context.PlaceMarketOrderAsync(new MarketOrder(figi, quantity, OperationType.Sell));

                    Console.WriteLine(@"Sell in short " + nameLot + " quantity: " + quantity + " price: " + closePrice);

                    ConsoleOutInformation(preCurrentIchResult, currentCandle, figi, nameLot, balance, closePrice);

                    using (StreamWriter sw = new StreamWriter("shoty", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(@"Sell in short " + nameLot + " quantity: " + quantity + " price: " + closePrice);
                    }
                }
                //Go to cash after short
                else if (EndShortSignal(currentIchResult, preCurrentIchResult, lotsFact, closePrice))
                {
                    await context.PlaceMarketOrderAsync(new MarketOrder(figi, lotsFact, OperationType.Buy));

                    Console.WriteLine(@"Buy from short " + nameLot + "quantity: " + lotsFact + " price: " + closePrice);
                }
                else
                {
                    ConsoleOutInformation(preCurrentIchResult, currentCandle, figi, nameLot, balance, closePrice);
                }

            }
        }

        private bool LongSignal(int lots, IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle, int lotsFact, decimal closePrice)
        {
            return
                lotsFact < lots
                && LongClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, closePrice);
            //&& LongClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle);
        }

        private bool EndLongSignal(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, int lotsFact, decimal closePrice)
        {
            return
                lotsFact > 0
                && EndLongClassicalCondition(currentIchResult, preCurrentIchResult, closePrice);
        }

        private bool ShortSignal(int lots, IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle, int lotsFact, decimal closePrice)
        {
            return lotsFact > 0 - lots
                            && ShortClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, closePrice);
        }

        private bool EndShortSignal(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, int lotsFact, decimal closePrice)
        {
            return
                lotsFact < 0
                && EndShortClassicalCondition(currentIchResult, preCurrentIchResult, closePrice);
        }

        public bool LongClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle, decimal closePrice)
        {
            return
                (currentIchResult.SenkouSpanA > currentIchResult.SenkouSpanB
                    || (currentIchResult.SenkouSpanA == currentIchResult.SenkouSpanB
                        && preCurrentIchResult.SenkouSpanA < preCurrentIchResult.SenkouSpanB))
                && (currentIchResult.TenkanSen > currentIchResult.KijunSen
                    || (currentIchResult.TenkanSen == currentIchResult.KijunSen
                        && preCurrentIchResult.TenkanSen < preCurrentIchResult.KijunSen))
                && (currentIchResult.TenkanSen > currentIchResult.SenkouSpanA
                    || (currentIchResult.TenkanSen == currentIchResult.SenkouSpanA
                        && preCurrentIchResult.TenkanSen < preCurrentIchResult.SenkouSpanA))
                && currentIchResult.TenkanSen > preCurrentIchResult.TenkanSen
                && (closePrice > currentIchResult.TenkanSen
                    || (closePrice == currentIchResult.TenkanSen
                        && preCurrentCandle.Close < preCurrentIchResult.TenkanSen))
                && closePrice > currentCandle.Open;
        }

        public bool LongClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle)
        {
            var closePrice = currentCandle.Close;
            return LongClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, closePrice);
        }

        public bool LongClassicalConditionForScreener(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle)
        {
            var closePrice = currentCandle.Close;
            return LongClassicalConditionForScreener(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, closePrice);
        }

        private bool LongClassicalConditionForScreener(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle, decimal closePrice)
        {
            return
                (currentIchResult.SenkouSpanA > currentIchResult.SenkouSpanB
                    || (currentIchResult.SenkouSpanA == currentIchResult.SenkouSpanB)
                && preCurrentIchResult.SenkouSpanA < preCurrentIchResult.SenkouSpanB)
                && (currentIchResult.TenkanSen > currentIchResult.KijunSen
                    || (currentIchResult.TenkanSen == currentIchResult.KijunSen
                        && preCurrentIchResult.TenkanSen < preCurrentIchResult.KijunSen))
                && (currentIchResult.TenkanSen > currentIchResult.SenkouSpanA
                    || (currentIchResult.TenkanSen == currentIchResult.SenkouSpanA
                        && preCurrentIchResult.TenkanSen < preCurrentIchResult.SenkouSpanA))
                && currentIchResult.TenkanSen > preCurrentIchResult.TenkanSen
                && (closePrice > currentIchResult.TenkanSen
                    || (closePrice == currentIchResult.TenkanSen
                        && preCurrentCandle.Close < preCurrentIchResult.TenkanSen))
                && closePrice > currentCandle.Open;
        }

        private bool EndLongClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, decimal closePrice)
        {
            return
                closePrice < currentIchResult.TenkanSen
                || closePrice < currentIchResult.KijunSen
                || closePrice < currentIchResult.SenkouSpanA
                || closePrice < currentIchResult.SenkouSpanB
                || currentIchResult.SenkouSpanA < preCurrentIchResult.SenkouSpanA;
        }

        public bool ShortClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle, decimal closePrice)
        {
            return
                (currentIchResult.SenkouSpanA < currentIchResult.SenkouSpanB
                    || (currentIchResult.SenkouSpanA == currentIchResult.SenkouSpanB
                        && preCurrentIchResult.SenkouSpanA > preCurrentIchResult.SenkouSpanB))
                && (currentIchResult.TenkanSen < currentIchResult.KijunSen
                    || (currentIchResult.TenkanSen == currentIchResult.KijunSen
                        && preCurrentIchResult.TenkanSen > preCurrentIchResult.KijunSen))
                && (currentIchResult.TenkanSen < currentIchResult.SenkouSpanA
                    || (currentIchResult.TenkanSen == currentIchResult.SenkouSpanA
                        && preCurrentIchResult.TenkanSen > preCurrentIchResult.SenkouSpanA))
                && currentIchResult.TenkanSen < preCurrentIchResult.TenkanSen
                && (closePrice < currentIchResult.TenkanSen
                    || (closePrice == currentIchResult.TenkanSen
                        && preCurrentCandle.Close > preCurrentIchResult.TenkanSen))
                && closePrice < currentCandle.Open;
        }

        public bool ShortClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, CandlePayload currentCandle, CandlePayload preCurrentCandle)
        {
            decimal closePrice = currentCandle.Close;
            return ShortClassicalCondition(currentIchResult, preCurrentIchResult, currentCandle, preCurrentCandle, closePrice);
        }

        private bool EndShortClassicalCondition(IchimokuResult currentIchResult, IchimokuResult preCurrentIchResult, decimal closePrice)
        {
            return
                closePrice > currentIchResult.TenkanSen
                || closePrice > currentIchResult.KijunSen
                || closePrice > currentIchResult.SenkouSpanA
                || closePrice > currentIchResult.SenkouSpanB
                || currentIchResult.SenkouSpanA > preCurrentIchResult.SenkouSpanA;
        }

        private void ConsoleOutInformation(IchimokuResult preCurrentIchResult, CandlePayload currentCandle, string figi, string nameLot, decimal balance, decimal closePrice)
        {
            Console.WriteLine(nameLot + " " + figi + ": " + preCurrentIchResult.TenkanSen + " TenkanSen");
            Console.WriteLine(nameLot + " " + figi + ": " + preCurrentIchResult.KijunSen + " KijunSen");
            Console.WriteLine(nameLot + " " + figi + ": " + preCurrentIchResult.SenkouSpanA + " SenkouSpanA");
            Console.WriteLine(nameLot + " " + figi + ": " + preCurrentIchResult.SenkouSpanB + " SenkouSpanB");
            Console.WriteLine(nameLot + " " + figi + ": " + currentCandle.Open + " Open");
            Console.WriteLine(nameLot + " " + figi + ": " + closePrice + " Close");
            Console.WriteLine(nameLot + " " + figi + "balance: " + balance);
        }
    }
}
