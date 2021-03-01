using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffTrade
{
    public class TransactionModel
    {
        public string Figi { get; set; }
        public decimal Price { get; set; }
        public decimal Margin { get; set; }
        public int Quantity { get; set; }
        public Operation Operation { get; set; }

    }
    public enum Operation
    {
        toLong,
        fromLong,
        toShort,
        fromShort,
        notTrading
    }
}

