using MarketDataModules;
using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffData;
using TradingAlgorithms.IndicatorSignals.Helpers;

namespace TradingAlgorithms.IndicatorSignals
{
    public partial class Signal : IndicatorSignalsHelper
    {

        internal bool OrderbookSignal(CandlesList candlesList, Orderbook orderbook)
        {
            int depthSmall = 5;
            int bidsdeth = 0;
            //int depthLarge = 20;
            if (orderbook.Bids.Count > 5)
            {
                bidsdeth = orderbook.Bids.Count - depthSmall;
            }
            else 
            {
                bidsdeth = orderbook.Bids.Count;
            }


            List<OrderbookEntry> asksSmall = orderbook.Asks.Take(depthSmall).ToList();
            List<OrderbookEntry> bidsSmall = orderbook.Bids.Skip(bidsdeth).ToList();
            List<OrderbookEntry> asksLarge = orderbook.Asks.ToList();
            List<OrderbookEntry> bidsLarge = orderbook.Bids.ToList();

            List<int> asksSmallQuantity = asksSmall.Select(x => x.Quantity).ToList();
            List<int> bidsSmallQuantity = bidsSmall.Select(x => x.Quantity).ToList();
            List<int> asksLargeQuantity = asksLarge.Select(x => x.Quantity).ToList();
            List<int> bidsLargeQuantity = bidsLarge.Select(x => x.Quantity).ToList();


            int asksSmallQuantitySum = asksSmallQuantity.Sum();
            int bidsSmallQuantitySum = bidsSmallQuantity.Sum();
            int asksLargeQuantitySum = asksLargeQuantity.Sum();
            int bidsLargeQuantitySum = bidsLargeQuantity.Sum();

            using (StreamWriter sw = new StreamWriter("_orderBook_" + orderbook.Figi, true, Encoding.Default))
            {
                sw.Write(DateTime.Now + " " + orderbook.Figi + " Asks: ");
                foreach (var item in asksLarge)
                {
                    sw.Write(" q: " + item.Quantity + " p: " + item.Price);
                }
                sw.WriteLine();
                sw.WriteLine();
                sw.Write(DateTime.Now + " " + orderbook.Figi + " Bids: ");
                foreach (var item in bidsLarge)
                {
                    sw.Write(" q: " + item.Quantity + " p: " + item.Price);
                }
                sw.WriteLine();
                sw.WriteLine("max price bid: " + bidsLarge.Max().Price);
                sw.WriteLine("max price ask: " + asksLarge.Max().Price);
                sw.WriteLine("asksSmallQuantitySum " + asksSmallQuantitySum);
                sw.WriteLine("bidsSmallQuantitySum " + bidsSmallQuantitySum);
                sw.WriteLine("asksLargeQuantitySum " + asksLargeQuantitySum);
                sw.WriteLine("bidsLargeQuantitySum " + bidsLargeQuantitySum);
                sw.WriteLine("***");
            }

            Log.Information("asksSmallQuantitySum " + asksSmallQuantitySum);
            Log.Information("bidsSmallQuantitySum " + bidsSmallQuantitySum);
            Log.Information("asksLargeQuantitySum " + asksLargeQuantitySum);
            Log.Information("bidsLargeQuantitySum " + bidsLargeQuantitySum);
            return true;

            //Log.Information("Start Orderbook LongSignal. Figi: " + candlesList.Figi);
            //if (
            //       depthSmall > 0 
            //   )
            //{


            //    Log.Information("Orderbook = Long - true for: " + candlesList.Figi);
            //    return true; 
            //}
            //else
            //{


            //    Log.Information("Orderbook = Long - falce for: " + candlesList.Figi);
            //    return false; 
            //}
        }


    }
}
