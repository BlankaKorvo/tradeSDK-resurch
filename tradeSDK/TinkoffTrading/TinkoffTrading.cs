using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffData;
using TradingAlgorithms.Algoritms;

namespace TinkoffTrade
{
    public class TinkoffTrading
    {
        public string figi { get; set; }
        public CandleInterval candleInterval { get; set; }
        public int countStoks { get; set; }
        public int CandleCount { get; set; } = 120;

        //время ожидания между следующим циклом
        int sleep { get; set; } = 0;


        Market market = new Market();
        SandboxContext context = new Auth().GetSanboxContext();
        //Mishmash mishmash = new Mishmash();
        public async Task PurchaseDecision()
        {
            Log.Information("Start PurchaseDecision");
            //Получаем свечи
            CandleList candleList = await market.GetCandlesTinkoff(context, figi, candleInterval, CandleCount);

            //Получаем стакан
            Orderbook orderbook = await GetOrderbook(figi, 1);
            decimal ask = orderbook.Asks.Last().Price;
            decimal bid = orderbook.Bids.Last().Price;
            decimal deltaPrice = (ask + bid) / 2;
            Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };

            if (mishmash.Long())
            { 
                BuyStoks(countStoks, ask);
                Log.Information("Go to Long: " + figi);
            }
            else if (mishmash.FromLong())
            { 
                SellStoksFromLong(bid);
                Log.Information("Go from Long: " + figi);
            }
            Log.Information("Stop PurchaseDecision");
        }

        private async Task<Orderbook> GetOrderbook(string figi, int depth)
        {
            Orderbook orderbook = await context.MarketOrderbookAsync(figi, depth);
            if (orderbook.Asks.Count == 0)
            {
                Log.Information("Биржа по инструменту " + figi + " не работает");
            }
            Log.Information("Orderbook Figi: " + orderbook.Figi);
            Log.Information("Orderbook Depth: " + orderbook.Depth);
            Log.Information("Orderbook Asks Price: " + orderbook.Asks.FirstOrDefault().Price);
            Log.Information("Orderbook Asks Quantity: " + orderbook.Asks.FirstOrDefault().Quantity);

            Log.Information("Orderbook Bids Price: " + orderbook.Bids.Last().Price);
            Log.Information("Orderbook Bids Quantity: " + orderbook.Bids.Last().Quantity);

            Log.Information("Orderbook ClosePrice: " + orderbook.ClosePrice);
            Log.Information("Orderbook LastPrice: " + orderbook.LastPrice);
            Log.Information("Orderbook LimitDown: " + orderbook.LimitDown);
            Log.Information("Orderbook LimitUp: " + orderbook.LimitUp);
            Log.Information("Orderbook TradeStatus: " + orderbook.TradeStatus);
            Log.Information("Orderbook MinPriceIncrement: " + orderbook.MinPriceIncrement);
            return orderbook;
        }

        private async void BuyStoks(int countLots, decimal price)
        {
            List<Order> orders = await context.OrdersAsync();
            foreach (Order order in orders)
            {
                if (order.Figi == figi)
                {
                    await context.CancelOrderAsync(order.OrderId);
                    Log.Information("Delete order by figi: " + figi + " RequestedLots " + order.RequestedLots + " ExecutedLots " + order.ExecutedLots + " Price " + order.Price + " Operation " + order.Operation + " Status " + order.Status + " Type " + order.Type);
                }
            }
            int countStocksBuyDeal = await CalculationStocksBuyDeal(figi, countLots);
            await context.PlaceLimitOrderAsync(new LimitOrder(figi, countStocksBuyDeal, OperationType.Buy, price));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Sell  price: " + price);
                sw.WriteLine();
            }
            Log.Information("Create order for Buy " + countLots + " lots " + "figi: " + figi + "price: " + price);
        }

        private async void SellStoksFromLong(decimal price)
        {
            List<Order> orders = await context.OrdersAsync();
            foreach (Order order in orders)
            {
                if (order.Figi == figi)
                {
                    await context.CancelOrderAsync(order.OrderId);
                    Log.Information("Delete order by figi: " + figi + " RequestedLots " + order.RequestedLots + " ExecutedLots " + order.ExecutedLots + " Price " + order.Price + " Operation " + order.Operation + " Status " + order.Status + " Type " + order.Type);
                }
            }
            int countLots = await CountLotsInPortfolio(figi);
            if (countLots == 0)
            { return; }
            await context.PlaceLimitOrderAsync(new LimitOrder(figi, countLots, OperationType.Sell, price));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Buy  price: " + price);
                sw.WriteLine();
            }
            Log.Information("Create order for Sell " + countLots + " stocks " + "figi: " + figi + "price: " + price);
        }
        
        private async Task<int> CalculationStocksBuyDeal(string figi, int countLotsToBuy)
        {
            int lots = await CountLotsInPortfolio(figi);
            Log.Information("Need to buy stocks: " + countLotsToBuy);

            int countLotsToBuyReal = countLotsToBuy - lots;
            Log.Information("Real need to buy: " + countLotsToBuyReal);

            return countLotsToBuyReal;
        }

        private async Task<int> CountLotsInPortfolio(string figi)
        {
            var portfolio = await context.PortfolioAsync();

            int lots = 0;
            foreach (var item in portfolio.Positions)
            {
                if (item.Figi == figi)
                {
                    lots = item.Lots;
                    Log.Information("Lots " + figi + " in portfolio: " + lots);
                    break;
                }
            };
            return lots;
        }
    }

}
