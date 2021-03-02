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
    public class TinkoffTrading : TransactionModel
    {
        public Context context { get; set; }
        public string figi { get; set; }
        public CandleInterval candleInterval { get; set; }
        //public int countStoks { get; set; }
        public int CandleCount { get; set; } = 120;
        //public decimal budget { get; set; }

        //время ожидания между следующим циклом
        int sleep { get; set; } = 0;

        Market market = new Market();
        TransactionModel transactionModel = new TransactionModel();

        public void Transaction(TransactionModel transactionModel)
        {
            if (
                transactionModel == null
                ||
                transactionModel.Figi == null
                ||
                transactionModel.Margin == 0
                ||
                transactionModel.Price == 0
                ||
                transactionModel.Quantity == 0)
            {
                Log.Information("Figi: " + transactionModel.Figi);
                Log.Information("Margin: " + transactionModel.Margin);
                Log.Information("Price: " + transactionModel.Price);
                Log.Information("Quantity: " + transactionModel.Quantity);
                Log.Information("Operation: " + transactionModel.Operation.ToString());
                Log.Error("Transaction is not correct for implementation");
                return;
            }

            switch (transactionModel.Operation)
            {
                case Operation.toLong:
                    BuyStoks(transactionModel);
                    Log.Information("Start Buy Stoks to Long");
                    break;

                case Operation.fromLong:
                    SellStoksFromLong(transactionModel);
                    Log.Information("Start Sell Stoks from Long");
                    break;

                case Operation.toShort:
                    //not implemented
                    Log.Error("Sell to short is not implemented");
                    break;
                case Operation.fromShort:
                    Log.Error("By from short is not implemented");
                    break;
            }
        }

        public async Task<TransactionModel> PurchaseDecision()
        {
            transactionModel.Figi = this.Figi;
            transactionModel.Margin = this.Margin;
            Log.Information("Start PurchaseDecision for: " + transactionModel.Figi);
            //Получаем свечи
            CandleList candleList = await market.GetCandlesTinkoffAsync(context, transactionModel.Figi, candleInterval, CandleCount);

            //Получаем стакан
            Orderbook orderbook = await market.GetOrderbook(context, transactionModel.Figi, 1);
            if (orderbook == null)
            {
                Log.Information("Orderbook " + transactionModel.Figi + " is null");
                transactionModel.Operation = Operation.notTrading;
                return transactionModel; 
            }
            decimal ask = orderbook.Asks.FirstOrDefault().Price;
            decimal bid = orderbook.Bids.FirstOrDefault().Price;
            int quantityAsksFirst = orderbook.Asks.FirstOrDefault().Quantity;
            int quantityBidsFirst = orderbook.Bids.FirstOrDefault().Quantity;
            decimal deltaPrice = (ask + bid) / 2;

            Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };

            if (mishmash.Long())
            {
                Log.Information("Go to Long: " + transactionModel.Figi);
                transactionModel.Quantity = quantityAsksFirst;
                transactionModel.Operation = Operation.toLong;
                transactionModel.Price = ask;
            }
            else if (mishmash.FromLong())
            {                
                Log.Information("Go from Long: " + transactionModel.Figi);
                transactionModel.Quantity = quantityBidsFirst;
                transactionModel.Operation = Operation.fromLong;
                transactionModel.Price = bid;
            }
            //Заглушка
            //else if(mishmash.Short())
            //{
            //    Log.Information("Go to Long: " + transactionModel.Figi);
            //    transactionModel.Quantity = quantityAsksFirst;
            //    transactionModel.Operation = Operation.toLong;
            //    transactionModel.Price = ask;
            //}
            //else if (mishmash.FromShort())
            //{
            //    Log.Information("Go from Long: " + transactionModel.Figi);
            //    transactionModel.Quantity = quantityBidsFirst;
            //    transactionModel.Operation = Operation.fromLong;
            //    transactionModel.Price = bid;
            //}
            Log.Information("Stop PurchaseDecision for: " + transactionModel.Figi);
            return transactionModel;
        }

        //private async Task<Orderbook> GetOrderbook(string figi, int depth)
        //{
        //    Orderbook orderbook = await context.MarketOrderbookAsync(figi, depth);
        //    if (orderbook.Asks.Count == 0 || orderbook.Bids.Count == 0)
        //    {
        //        Log.Information("Exchange by instrument " + figi + " not working");
        //        return null;
        //    }
        //    Log.Information("Orderbook Figi: " + orderbook.Figi);
        //    Log.Information("Orderbook Depth: " + orderbook.Depth);
        //    Log.Information("Orderbook Asks Price: " + orderbook.Asks.FirstOrDefault().Price);
        //    Log.Information("Orderbook Asks Quantity: " + orderbook.Asks.FirstOrDefault().Quantity);

        //    Log.Information("Orderbook Bids Price: " + orderbook.Bids.FirstOrDefault().Price);
        //    Log.Information("Orderbook Bids Quantity: " + orderbook.Bids.FirstOrDefault().Quantity);

        //    Log.Information("Orderbook ClosePrice: " + orderbook.ClosePrice);
        //    Log.Information("Orderbook LastPrice: " + orderbook.LastPrice);
        //    Log.Information("Orderbook LimitDown: " + orderbook.LimitDown);
        //    Log.Information("Orderbook LimitUp: " + orderbook.LimitUp);
        //    Log.Information("Orderbook TradeStatus: " + orderbook.TradeStatus);
        //    Log.Information("Orderbook MinPriceIncrement: " + orderbook.MinPriceIncrement);
        //    return orderbook;
        //}

        private async void BuyStoks(TransactionModel transactionModel)
        {
            Log.Information("Start BuyStoks: " + transactionModel.Figi);
            List<Order> orders = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.OrdersAsync());
            foreach (Order order in orders)
            {
                if (order.Figi == transactionModel.Figi)
                {
                    await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.CancelOrderAsync(order.OrderId));
                    Log.Information("Delete order by figi: " + transactionModel.Figi + " RequestedLots " + order.RequestedLots + " ExecutedLots " + order.ExecutedLots + " Price " + order.Price + " Operation " + order.Operation + " Status " + order.Status + " Type " + order.Type);
                }
            }
            int lots = await CalculationLotsByMargin(transactionModel);
            //transactionModel.Quantity = await CalculationLotsByMargin(transactionModel);
            if (lots == 0)
            {
                Log.Information("Not any lot in margin: " + transactionModel.Margin);
                return; }

            await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.PlaceLimitOrderAsync(new LimitOrder(transactionModel.Figi, lots, OperationType.Buy, transactionModel.Price)));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Buy " + transactionModel.Figi + " Quantity: " + transactionModel.Quantity +  " price: " + transactionModel.Price + " mzda: " + (transactionModel.Price * 0.02m) / 100m);
                sw.WriteLine();
            }
            Log.Information("Create order for Buy " + lots + " lots " + "figi: " + transactionModel.Figi + "price: " + transactionModel.Price);
            Log.Information("Stop BuyStoks: " + transactionModel.Figi);
        }

        private async void SellStoksFromLong(TransactionModel transactionModel)
        {
            Log.Information("Start SellStoksFromLong: " + transactionModel.Figi);
            int lots = await CalculationStocksFromLong(transactionModel);
            if (lots == 0)
            { return; }
            await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.PlaceLimitOrderAsync(new LimitOrder(transactionModel.Figi, lots, OperationType.Sell, transactionModel.Price)));
            using (StreamWriter sw = new StreamWriter("operation", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + @" Sell " + transactionModel.Figi + "Quantity: " + transactionModel.Quantity + " price: " + transactionModel.Price + " mzda: " + (transactionModel.Price * 0.02m) / 100m);
                sw.WriteLine();
            }
            Log.Information("Create order for Sell " + lots + " stocks " + "figi: " + transactionModel.Figi + "price: " + transactionModel.Price);
            Log.Information("Stop SellStoksFromLong: " + transactionModel.Figi);
        }
        
        private async Task<int> CalculationStocksBuyCount(string figi, int countLotsToBuy)
        {
            int lots = await CountLotsInPortfolio(figi);
            Log.Information("Need to buy stocks: " + countLotsToBuy);

            int countLotsToBuyReal = countLotsToBuy - lots;
            Log.Information("Real need to buy: " + countLotsToBuyReal);

            return countLotsToBuyReal;
        }
        private async Task<int> CalculationLotsByMargin(TransactionModel transactionModel)
        {
            int lots = await CountLotsInPortfolio(transactionModel.Figi);
            Log.Information("Lots " + transactionModel.Figi + " in portfolio: " + lots);
            decimal sumCostLotsInPorfolio = transactionModel.Price * Convert.ToDecimal(lots);
            decimal remainingCostLots = transactionModel.Margin - sumCostLotsInPorfolio;
            if (remainingCostLots <= 0)
            {
                return 0;
            }
            int countLotsToBuy = Convert.ToInt32(Math.Floor(remainingCostLots / transactionModel.Price));
            if (countLotsToBuy <= transactionModel.Quantity)
            {
                Log.Information("Need to buy: " + countLotsToBuy);
                return countLotsToBuy;
            }
            else
            {
                Log.Information("Need to buy: " + transactionModel.Quantity);
                return transactionModel.Quantity;
            }
        }

        private async Task<int> CalculationStocksFromLong(TransactionModel transactionModel)
        {
            int lots = await CountLotsInPortfolio(transactionModel.Figi);
            Log.Information("Lots " + transactionModel.Figi + " in portfolio: " + lots);
            if (lots <= transactionModel.Quantity)
            {
                Log.Information("Need to sell: " + lots);
                return lots;
            }
            else
            {
                Log.Information("Need to buy: " + transactionModel.Quantity);
                return transactionModel.Quantity;
            }
        }
        private async Task<int> CountLotsInPortfolio(string figi)
        {
            var portfolio = await RetryPolicy.Model.RetryToManyReq().ExecuteAsync(async () => await context.PortfolioAsync());
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
