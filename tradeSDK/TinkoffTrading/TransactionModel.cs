using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffTrade
{
    public class TransactionModel
    {
        internal string Figi { get; set; }
        internal decimal Price { get; set; }
        internal int Quantity { get; set; }
        internal Operation Operation { get; set; }

    }
    enum Operation
    {
        toLong,
        fromLong,
        toShort,
        fromShort
    }
}

