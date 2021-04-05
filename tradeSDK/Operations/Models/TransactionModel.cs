using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffAdapter
{
    public class TransactionModel
    {
        public string Figi { get; set; }
        public decimal Price { get; set; }
        public decimal Purchase { get; set; } //объем покупки в валюте
        public int Quantity { get; set; } //кол-во акций
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

