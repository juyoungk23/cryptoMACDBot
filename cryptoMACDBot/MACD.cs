using System;
using System.Linq;
using System.Collections.Generic;
using Skender.Stock.Indicators;

namespace cryptoMACDBot
{
    public class MACD
    {
        public MACD(List<Candle> candles)
        {
            this.candles = candles;
        }

        public int shortPeriod = 12;
        public int longPeriod = 26;

        public IEnumerable<MacdResult> GetMACD()
        {
            return candles.GetMacd(shortPeriod, longPeriod);
        }

        public List<Candle> candles;

        public double GetMACDblue()
        {
            return (double) GetMACD().Last().Macd;
        }

        public double GetMACDorange()
        {
            return (double) GetMACD().Last().Signal;
        }
    }
}

