using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffAdapter.DataHelper
{
    class ComparerTinkoffCandlePayloadEquality : IEqualityComparer<CandlePayload>
    {
        public bool Equals(CandlePayload c1, CandlePayload c2)
        {
            Log.Information("Start Equals method");
            if (c1.Time == c2.Time)
            {
                Log.Information(c1.Figi + " " + c1.Time + " candle = " + c2.Figi + " " + c2.Time + " candle");
                Log.Information("Stop Equals method. Return true");
                return true;
            }
            else
            {
                Log.Information(c1.Figi + " " + c1.Time + " candle != " + c2.Figi + " " + c2.Time + " candle");
                Log.Information("Stop Equals method. Return falce");
                return false;
            }
        }

        public int GetHashCode(CandlePayload c)
        {
            string hCode = (c.High  * c.Low * c.Open).ToString() +c.Figi + c.Interval.ToString() + c.Time.ToString();
            return hCode.GetHashCode();
        }
    }
}
