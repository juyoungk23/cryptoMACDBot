using System;
using System.Linq;
using System.Collections.Generic;
using Skender.Stock.Indicators;

namespace cryptoMACDBot
{
	public class EMA
	{

		private List<Candle> candles;
		private int lookbackPeriod;

		public EMA(List<Candle> candles, int lookbackPeriod)
		{
			this.candles = candles;
			this.lookbackPeriod = lookbackPeriod;
		}

		public IEnumerable<EmaResult> GetEMA()
        {
			return candles.GetEma(lookbackPeriod);
		}

        public decimal GetLatestEMA()
        {
			return (decimal) GetEMA().Last().Ema;
        }
    }
}

