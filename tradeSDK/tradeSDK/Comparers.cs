using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace tradeSDK
{
    class CandlePayloadEqualityComparer : IEqualityComparer<CandlePayload>
    {
        public bool Equals(CandlePayload c1, CandlePayload c2)
        {
            if (c1.Time == c2.Time)
                return true;
            else
                return false;
        }

        public int GetHashCode(CandlePayload c)
        {
            string hCode = (c.High  * c.Low * c.Open).ToString() +c.Figi + c.Interval.ToString() + c.Time.ToString();
            return hCode.GetHashCode();
        }
    }
}
