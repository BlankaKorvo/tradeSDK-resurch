using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataModules
{
    public class TransactionModel : TransactionModelBase
    {
        public int Quantity { get; set; } //кол-во акций
        public decimal Purchase { get; set; } //объем покупки в валюте
    }

    public class TransactionModelBase
    {
        public string Figi { get; set; }
        public decimal Price { get; set; }
        public Operation Operation { get; set; }

    }
    public enum Operation
    {
        notTrading,
        toLong,
        fromLong,
        toShort,
        fromShort        
    }
}

