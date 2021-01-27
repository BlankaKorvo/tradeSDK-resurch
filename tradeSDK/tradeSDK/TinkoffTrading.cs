﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TradingAlgorithms.Algoritms;

namespace tradeSDK
{
    internal class TinkoffTrading
    {
        string figi { get; set; }
        CandleInterval candleInterval { get; set; }
        int CandleCount { get; set; }

        //время ожидания между следующим циклом
        int sleep { get; set; } = 0;



        Market market = new Market();
        SandboxContext context = new Auth().GetSanboxContext();
        Mishmash mishmash = new Mishmash();
        internal async Task PurchaseDecision()
        {
            Log.Information("Start PurchaseDecision");
            //Получаем свечи
            CandleList candleList = await market.GetCandlesTinkoff(context, figi, candleInterval, CandleCount);

           //Получаем стакан
            Orderbook orderbook = await context.MarketOrderbookAsync(figi, 1);
            if (orderbook.Asks.Count == 0)
            {
                Log.Information("Биржа по инструменту " + figi + " не работает");
            }
            decimal ask = orderbook.Asks.Last().Price;
            decimal bid = orderbook.Bids.Last().Price;
            decimal deltaPrice = (ask + bid) / 2;
            Mishmash mishmash = new Mishmash() { candleList = candleList, deltaPrice = deltaPrice };
            if (mishmash.Long())
            { BuyStoks(); }
            else if (mishmash.FromLong())
            { SellStoks(); }
            Log.Information("Stop PurchaseDecision");
        }

        private void SellStoks()
        {
            throw new NotImplementedException();
        }

        private void BuyStoks()
        {
            throw new NotImplementedException();
        }
    }
}
