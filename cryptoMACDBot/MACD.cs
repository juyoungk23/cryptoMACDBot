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

        public int fastPeriods = 12;
        public int slowPeriods = 26;
        public int signalPeriods = 9;

        public List<Candle> candles = new List<Candle>();

        public void Update(List<Candle> candles)
        {
            this.candles = candles;
        }

        public IEnumerable<MacdResult> GetMACDResults()
        {
            return candles.GetMacd(fastPeriods, slowPeriods, signalPeriods);
        }

        public decimal? GetMACDblue()
        {
            return (decimal?) GetMACDResults().Last().Macd;
        }

        public decimal? GetMACDblueAtIndex(int index)
        {
            return (decimal?)GetMACDResults().ElementAt(index).Macd;
        }

        public decimal? GetMACDorange()
        {
           return (decimal?) GetMACDResults().Last().Signal;
           
        }

        public decimal? GetMACDorangeAtIndex(int index)
        {
            return (decimal?)GetMACDResults().ElementAt(index).Signal;
        }



    }
}

