using System;
using System.Linq;
using System.Collections.Generic;
using Skender.Stock.Indicators;

namespace cryptoMACDBot
{
	public class EMA
	{

		public List<Candle> candles;
		private int lookbackPeriod;

		public EMA(List<Candle> candles, int lookbackPeriod)
		{
			this.candles = candles;
			this.lookbackPeriod = lookbackPeriod;
		}

		public void Update(List<Candle> candles)
		{
			this.candles = candles;
		}

		private IEnumerable<EmaResult> GetEMAResults()
        {
			return candles.GetEma(lookbackPeriod);
		}

        public decimal GetLatestEMA()
        {
			return (decimal) GetEMAResults().Last().Ema;
        }

		public decimal GetEMAatIndex(int index)
        {
			return (decimal) GetEMAResults().ElementAt(index).Ema;
        }
    }
}

