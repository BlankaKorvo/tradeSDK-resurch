using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffData;
using TradingAlgorithms.Algoritms;

namespace TinkoffTrade
{
    public class TinkoffTrading 
    {
        public SandboxContext context { get; set; }
        public string figi { get; set; }
        public CandleInterval candleInterval { get; set; }
        public int countStoks { get; set; }
        public int CandleCount { get; set; } = 120;
        public decimal budget { get; set; }

        //время ожидания между следующим циклом
        int sleep { get; set; } = 0;


        Market market = new Market();
        //SandboxContext context = new Auth().GetSanboxContext();
        //Mishmash mishmash = new Mishmash();
        public async Task<TransactionModel> PurchaseDecision()
        {
            TransactionModel transactionModel = new TransactionModel();
            Log.Information("Start PurchaseDecision for: " + figi);
            //Получаем свечи
            CandleList candleList = await market.GetCandlesTinkoff(context, figi, candleInterval, CandleCount);

            //Получаем стакан
            Orderbook orderbook = await GetOrderbook(figi, 1);
            if (orderbook == null)
            {
                Log.Information("Orderbook " + figi + " is null");
                return null; 
            }
            decimal ask = orderbook.Asks.FirstOrDefault().Price;
            decimal bid = orderbook.Bids.FirstOrDefault().Price;
            int quantityAsksFirst = orderbook.Asks.FirstOrDefault().Quantity;
            int quantityBidsFirst = orderbook.Bids.FirstOrDefault().Quantity;
            decimal deltaPrice = (ask + bid) / 2;

            transactionModel.Price = (ask + bid) / 2;
            transactionModel.Figi = figi;
            Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };

            if (mishmash.Long())
            {
                //BuyStoks(countStoks, ask);
                BuyStoks(budget, quantityAsksFirst, ask);
                Log.Information("Go to Long: " + figi);
                transactionModel.Quantity = quantityAsksFirst;
                transactionModel.Operation = Operation.toLong;
                return transactionModel;
            }
            else if (mishmash.FromLong())
            {
                SellStoksFromLong(bid, quantityBidsFirst);
                Log.Information("Go from Long: " + figi);
                transactionModel.Quantity = quantityBidsFirst;
                transactionModel.Operation = Operation.fromLong;
                return transactionModel;
            }
            Log.Information("Stop PurchaseDecision for: " + figi);
            return null;
        }

        private async Task<Orderbook> GetOrderbook(string figi, int depth)
        {
            Orderbook orderbook = await context.MarketOrderbookAsync(figi, depth);
            if (orderbook.Asks.Count == 0 || orderbook.Bids.Count == 0)
            {
                Log.Information("Exchange by instrument " + figi + " not working");
                return null;
            }
            Log.Information("Orderbook Figi: " + orderbook.Figi);
            Log.Information("Orderbook Depth: " + orderbook.Depth);
            Log.Information("Orderbook Asks Price: " + orderbook.Asks.FirstOrDefault().Price);
            Log.Information("Orderbook Asks Quantity: " + orderbook.Asks.FirstOrDefault().Quantity);

            Log.Information("Orderbook Bids Price: " + orderbook.Bids.FirstOrDefault().Price);
            Log.Information("Orderbook Bids Quantity: " + orderbook.Bids.FirstOrDefault().Quantity);

            Log.Information("Orderbook ClosePrice: " + orderbook.ClosePrice);
            Log.Information("Orderbook LastPrice: " + orderbook.LastPrice);
            Log.Information("Orderbook LimitDown: " + orderbook.LimitDown);
            Log.Information("Orderbook LimitUp: " + orderbook.LimitUp);
            Log.Information("Orderbook TradeStatus: " + orderbook.TradeStatus);
            Log.Information("Orderbook MinPriceIncrement: " + orderbook.MinPriceIncrement);
            return orderbook;
        }

        //private async void BuyStoks(int countLots, decimal price)
        //{
        //    List<Order> orders = await context.OrdersAsync();
        //    foreach (Order order in orders)
        //    {
        //        if (order.Figi == figi)
        //        {
        //            await context.CancelOrderAsync(order.OrderId);
        //            Log.Information("Delete order by figi: " + figi + " RequestedLots " + order.RequestedLots + " ExecutedLots " + order.ExecutedLots + " Price " + order.Price + " Operation " + order.Operation + " Status " + order.Status + " Type " + order.Type);
        //        }
        //    }
        //    int countStocksBuyDeal = await CalculationStocksBuyCount(figi, countLots);
        //    if (countStocksBuyDeal == 0)
        //    { return; }
        //    await context.PlaceLimitOrderAsync(new LimitOrder(figi, countStocksBuyDeal, OperationType.Buy, price));
        //    using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
        //    {
        //        sw.WriteLine(DateTime.Now + @" Buy " + figi + " price: " + price + " mzda: " + (price*0.02m)/100m);
        //        sw.WriteLine();
        //    }
        //    Log.Information("Create order for Buy " + countLots + " lots " + "figi: " + figi + "price: " + price);
        //}

        private async void BuyStoks(decimal budget, int quantityAskFirst, decimal price)
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
            int stocksBuyBudget = await CalculationStocksBuyBudget(figi, budget, quantityAskFirst, price);
            if (stocksBuyBudget == 0)
            { return; }

            await context.PlaceLimitOrderAsync(new LimitOrder(figi, stocksBuyBudget, OperationType.Buy, price));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Buy " + figi + " price: " + price + " mzda: " + (price * 0.02m) / 100m);
                sw.WriteLine();
            }
            Log.Information("Create order for Buy " + stocksBuyBudget + " lots " + "figi: " + figi + "price: " + price);
        }

        private async void SellStoksFromLong(decimal bid, int quantityBidsFirst)
        {
            int countLots = await CalculationStocksFromLong(figi, quantityBidsFirst);
            if (countLots == 0)
            { return; }
            await context.PlaceLimitOrderAsync(new LimitOrder(figi, countLots, OperationType.Sell, bid));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Sell " + figi + " price: " + bid + " mzda: " + (bid * 0.02m) / 100m);
                sw.WriteLine();
            }
            Log.Information("Create order for Sell " + countLots + " stocks " + "figi: " + figi + "price: " + bid);
        }
        
        private async Task<int> CalculationStocksBuyCount(string figi, int countLotsToBuy)
        {
            int lots = await CountLotsInPortfolio(figi);
            Log.Information("Need to buy stocks: " + countLotsToBuy);

            int countLotsToBuyReal = countLotsToBuy - lots;
            Log.Information("Real need to buy: " + countLotsToBuyReal);

            return countLotsToBuyReal;
        }
        private async Task<int> CalculationStocksBuyBudget(string figi, decimal budget, int quantityAskFirst, decimal price)
        {
            int lots = await CountLotsInPortfolio(figi);
            Log.Information("Lots " + figi + " in portfolio: " + lots);
            decimal sumCostLotsInPorfolio = price * Convert.ToDecimal(lots);
            decimal remainingCostLots = budget - sumCostLotsInPorfolio;
            int countLotsToBuy = Convert.ToInt32(Math.Floor(remainingCostLots / price));
            if (countLotsToBuy <= quantityAskFirst)
            {
                Log.Information("Need to buy: " + countLotsToBuy);
                return countLotsToBuy;
            }
            else
            {
                Log.Information("Need to buy: " + quantityAskFirst);
                return quantityAskFirst;
            }
        }

        private async Task<int> CalculationStocksFromLong(string figi, int quantityBidFirst)
        {
            int lots = await CountLotsInPortfolio(figi);
            Log.Information("Lots " + figi + " in portfolio: " + lots);
            if (lots <= quantityBidFirst)
            {
                Log.Information("Need to sell: " + lots);
                return lots;
            }
            else
            {
                Log.Information("Need to buy: " + quantityBidFirst);
                return quantityBidFirst;
            }
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
